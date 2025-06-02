namespace SLC_SM_IAS_Service_Spec_Configuration.Presenters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;

	using DomHelpers.SlcConfigurations;
	using DomHelpers.SlcServicemanagement;

	using Library;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	using SLC_SM_IAS_Service_Spec_Configuration.Views;

	public class ServiceConfigurationPresenter
	{
		private readonly List<DataRecord> configurations = new List<DataRecord>();
		private readonly DomHelper domHelperConfig;
		private readonly DomHelper domHelperSrvMgmt;
		private readonly IEngine engine;
		private readonly InteractiveController controller;
		private readonly ServiceSpecificationsInstance instance;
		private readonly ServiceConfigurationView view;
		private RepoConfigurations repoConfig;

		public ServiceConfigurationPresenter(IEngine engine, InteractiveController controller, ServiceConfigurationView view, ServiceSpecificationsInstance instance)
		{
			this.engine = engine;
			this.controller = controller;
			this.view = view;
			this.instance = instance;

			domHelperSrvMgmt = new DomHelper(engine.SendSLNetMessages, SlcServicemanagementIds.ModuleId);
			domHelperConfig = new DomHelper(engine.SendSLNetMessages, SlcConfigurationsIds.ModuleId);

			view.BtnCancel.Pressed += (sender, args) => throw new ScriptAbortException("OK");
			view.BtnUpdate.Pressed += (sender, args) =>
			{
				StoreModels();
				throw new ScriptAbortException("OK");
			};
		}

		private enum State
		{
			Update,
			Delete,
		}

		public void LoadFromModel()
		{
			var currentConfiguration = GetCurrentConfigurationParameters();
			repoConfig = new RepoConfigurations(domHelperConfig);

			foreach (var currentConfig in currentConfiguration)
			{
				var configParamValue = domHelperConfig.GetConfigurationParameterValueInstance(currentConfig.ServiceSpecificationConfigurationValue.ConfigurationParameterValue);
				if (configParamValue == null)
				{
					continue;
				}

				var configParam = domHelperConfig.GetConfigurationParameterInstance(configParamValue.ConfigurationParameterValue.ConfigurationParameterReference);
				if (configParam == null)
				{
					continue;
				}

				DataRecord dataRecord = BuildDataRecord(currentConfig, configParamValue, configParam);
				configurations.Add(dataRecord);
			}

			BuildUI();
		}

		public void StoreModels()
		{
			foreach (var configuration in configurations)
			{
				if (configuration.State == State.Delete)
				{
					configuration.NumberParameterOptions?.Delete(domHelperConfig);
					configuration.DiscreteParameterOptions?.Delete(domHelperConfig);
					configuration.TextParameterOptions?.Delete(domHelperConfig);
					configuration.ConfigurationParamValue.Delete(domHelperConfig);
					configuration.ServiceConfig.Delete(domHelperSrvMgmt);
				}
				else
				{
					configuration.NumberParameterOptions?.Save(domHelperConfig);
					configuration.DiscreteParameterOptions?.Save(domHelperConfig);
					configuration.TextParameterOptions?.Save(domHelperConfig);
					configuration.ConfigurationParamValue.Save(domHelperConfig);
					configuration.ServiceConfig.Save(domHelperSrvMgmt);
				}
			}

			instance.Save(domHelperSrvMgmt);
		}

		private void AddConfigModel(ConfigurationParametersInstance selectedParameter)
		{
			var configurationParameterInstance = selectedParameter ?? new ConfigurationParametersInstance();
			var configurationParameterValueInstance = new ConfigurationParameterValueInstance
			{
				ConfigurationParameterValue = new ConfigurationParameterValueSection
				{
					Label = String.Empty,
					Type = configurationParameterInstance.ConfigurationParameterInfo.Type,
					ConfigurationParameterReference = configurationParameterInstance.ID.Id,
				},
			};
			var serviceSpecificationConfigurationValueInstance = new ServiceSpecificationConfigurationValueInstance
			{
				ServiceSpecificationConfigurationValue = new ServiceSpecificationConfigurationValueSection
				{
					ExposeAtServiceOrderLevel = true,
					MandatoryAtServiceLevel = true,
					MandatoryAtServiceOrderLevel = true,
					ConfigurationParameterValue = configurationParameterValueInstance.ID.Id,
				},
			};
			instance.ServiceSpecificationInfo.ServiceSpecificationConfigurationParameters.Add(serviceSpecificationConfigurationValueInstance.ID.Id);

			configurations.Add(BuildDataRecord(serviceSpecificationConfigurationValueInstance, configurationParameterValueInstance, configurationParameterInstance));
		}

		private DataRecord BuildDataRecord(
			ServiceSpecificationConfigurationValueInstance currentConfig,
			ConfigurationParameterValueInstance configParamValue,
			ConfigurationParametersInstance configParam)
		{
			var dataRecord = new DataRecord
			{
				State = State.Update,
				ServiceConfig = currentConfig,
				ConfigurationParamValue = configParamValue,
				ConfigurationParam = configParam,
			};

			// Foresee a copy of the number/discrete parameter options starting from the default
			if (configParamValue.ConfigurationParameterValue.Type == SlcConfigurationsIds.Enums.ParameterType.Number)
			{
				if (!configParamValue.ConfigurationParameterValue.NumberValueOptions.HasValue)
				{
					// Create duplicate of default parameter options linked to the parameterValueInstance
					var numberOption = repoConfig.NumberParameterOptions.Find(x => x.ID.Id == configParam.ConfigurationParameterInfo.NumberOptions);
					dataRecord.NumberParameterOptions = numberOption != null
						? new NumberParameterOptionsInstance { NumberParameterOptions = numberOption.NumberParameterOptions }
						: new NumberParameterOptionsInstance();
				}
				else
				{
					dataRecord.NumberParameterOptions = repoConfig.NumberParameterOptions.Find(x => x.ID.Id == configParamValue.ConfigurationParameterValue.NumberValueOptions)
														?? new NumberParameterOptionsInstance();
				}

				dataRecord.ConfigurationParamValue.ConfigurationParameterValue.NumberValueOptions = dataRecord.NumberParameterOptions.ID.Id;
			}
			else if (configParamValue.ConfigurationParameterValue.Type == SlcConfigurationsIds.Enums.ParameterType.Discrete)
			{
				if (!configParamValue.ConfigurationParameterValue.DiscreteValueOptions.HasValue)
				{
					// Create duplicate of default parameter options linked to the parameterValueInstance
					var discreteOption = repoConfig.DiscreteParameterOptions.Find(x => x.ID.Id == configParam.ConfigurationParameterInfo.DiscreteOptions);
					dataRecord.DiscreteParameterOptions = discreteOption != null
						? new DiscreteParameterOptionsInstance { DiscreteParameterOptions = discreteOption.DiscreteParameterOptions }
						: new DiscreteParameterOptionsInstance();
				}
				else
				{
					dataRecord.DiscreteParameterOptions = repoConfig.DiscreteParameterOptions.Find(x => x.ID.Id == configParamValue.ConfigurationParameterValue.DiscreteValueOptions)
														  ?? new DiscreteParameterOptionsInstance();
				}

				dataRecord.ConfigurationParamValue.ConfigurationParameterValue.DiscreteValueOptions = dataRecord.DiscreteParameterOptions.ID.Id;
			}
			else
			{
				if (!configParamValue.ConfigurationParameterValue.TextValueOptions.HasValue)
				{
					// Create duplicate of default parameter options linked to the parameterValueInstance
					var textOption = repoConfig.TextParameterOptions.Find(x => x.ID.Id == configParam.ConfigurationParameterInfo.TextOptions);
					dataRecord.TextParameterOptions = textOption != null
						? new TextParameterOptionsInstance { TextParameterOptions = textOption.TextParameterOptions }
						: new TextParameterOptionsInstance();
				}
				else
				{
					dataRecord.TextParameterOptions = repoConfig.TextParameterOptions.Find(x => x.ID.Id == configParamValue.ConfigurationParameterValue.TextValueOptions)
													  ?? new TextParameterOptionsInstance();
				}

				dataRecord.ConfigurationParamValue.ConfigurationParameterValue.TextValueOptions = dataRecord.TextParameterOptions.ID.Id;
			}

			return dataRecord;
		}

		private void BuildHeaderRow(int row)
		{
			var lblLabel = new Label("Label");
			var lblParameter = new Label("Parameter");
			var lblLink = new Label("Link");
			var lblValue = new Label("Value");
			var lblUnit = new Label("Unit");
			var lblStart = new Label("Start");
			var lblEnd = new Label("End");
			var lblStop = new Label("Step Size");
			var lblDecimals = new Label("Decimals");
			var lblValues = new Label("Values");
			var lblDefault = new Label("Default");
			var lblExposeAtOrder = new Label("Expose At Order");
			var lblMandatoryAtOrder = new Label("Mandatory At Order");
			var lblMandatoryAtService = new Label("Mandatory At Service");

			view.AddWidget(lblLabel, row, 0);
			view.AddWidget(lblParameter, row, 1);
			view.AddWidget(lblLink, row, 2);
			view.AddWidget(lblValue, row, 3);
			view.AddWidget(lblUnit, row, 4);
			view.AddWidget(lblStart, row, 5);
			view.AddWidget(lblEnd, row, 6);
			view.AddWidget(lblStop, row, 7);
			view.AddWidget(lblDecimals, row, 8);
			view.AddWidget(lblValues, row, 9);
			view.AddWidget(lblDefault, row, 10);
			view.AddWidget(lblExposeAtOrder, row, 11);
			view.AddWidget(lblMandatoryAtOrder, row, 12);
			view.AddWidget(lblMandatoryAtService, row, 13);
		}

		private void BuildUI()
		{
			view.Clear();

			int row = 0;
			view.AddWidget(view.TitleDetails, row, 0);

			BuildHeaderRow(++row);

			foreach (var configuration in configurations.Where(x => x.State != State.Delete))
			{
				BuildUIRow(configuration, ++row);
			}

			view.AddWidget(new WhiteSpace(), ++row, 0);
			var parameter = new DropDown<ConfigurationParametersInstance>(
				repoConfig.ConfigurationParameters.Select(x => new Option<ConfigurationParametersInstance>(x.ConfigurationParameterInfo.ParameterName, x)).OrderBy(x => x.DisplayValue));
			view.AddWidget(parameter, ++row, 1);
			var btnAdd = new Button("➕");
			btnAdd.Pressed += (sender, args) =>
			{
				AddConfigModel(parameter.Selected);
				BuildUI();
			};
			view.AddWidget(btnAdd, row, 2);

			view.AddWidget(new WhiteSpace(), ++row, 0);
			view.AddWidget(view.BtnCancel, ++row, 0);
			view.AddWidget(view.BtnUpdate, row, 1);
		}

		private void BuildUIRow(DataRecord record, int row)
		{
			// Init
			var label = new TextBox(record.ConfigurationParamValue.ConfigurationParameterValue.Label);
			var parameter = new DropDown<ConfigurationParametersInstance>(
				new[] { new Option<ConfigurationParametersInstance>(record.ConfigurationParam.ConfigurationParameterInfo.ParameterName, record.ConfigurationParam) })
			{
				IsEnabled = false,
			};
			var link = new CheckBox { IsChecked = record.ConfigurationParamValue.ConfigurationParameterValue.LinkedInstanceReference != null };
			var unit = new Label("-");
			var start = new Numeric { IsEnabled = false };
			var end = new Numeric { IsEnabled = false };
			var step = new Numeric { IsEnabled = false };
			var decimals = new Numeric { StepSize = 1, Minimum = 0, Maximum = 6, IsEnabled = false, Width = 70 };
			var exposeAtOrder = new CheckBox { IsChecked = record.ServiceConfig.ServiceSpecificationConfigurationValue.ExposeAtServiceOrderLevel ?? true };
			var mandatoryAtOrder = new CheckBox { IsChecked = record.ServiceConfig.ServiceSpecificationConfigurationValue.MandatoryAtServiceOrderLevel ?? true };
			var mandatoryAtService = new CheckBox { IsChecked = record.ServiceConfig.ServiceSpecificationConfigurationValue.MandatoryAtServiceLevel ?? true };
			var delete = new Button("🚫");

			label.Changed += (sender, args) => record.ConfigurationParamValue.ConfigurationParameterValue.Label = args.Value;
			exposeAtOrder.Changed += (sender, args) => record.ServiceConfig.ServiceSpecificationConfigurationValue.ExposeAtServiceOrderLevel = args.IsChecked;
			mandatoryAtOrder.Changed += (sender, args) => record.ServiceConfig.ServiceSpecificationConfigurationValue.MandatoryAtServiceOrderLevel = args.IsChecked;
			mandatoryAtService.Changed += (sender, args) => record.ServiceConfig.ServiceSpecificationConfigurationValue.MandatoryAtServiceLevel = args.IsChecked;
			delete.Pressed += (sender, args) =>
			{
				record.State = State.Delete;
				instance.ServiceSpecificationInfo.ServiceSpecificationConfigurationParameters.Remove(record.ServiceConfig.ID.Id);
				BuildUI();
			};
			link.Changed += (sender, args) =>
			{
				record.ConfigurationParamValue.ConfigurationParameterValue.LinkedInstanceReference = args.IsChecked ? "Dummy Link" : null;
				BuildUI();
			};

			if (record.ConfigurationParamValue.ConfigurationParameterValue.LinkedInstanceReference != null)
			{
				view.AddWidget(new DropDown(), row, 3);
				view.AddWidget(new WhiteSpace(), row, 9);
				view.AddWidget(new WhiteSpace(), row, 10);
			}
			else
			{
				switch (parameter.Selected.ConfigurationParameterInfo.Type)
				{
					case SlcConfigurationsIds.Enums.ParameterType.Number:
					{
						double minimum = record.NumberParameterOptions.NumberParameterOptions.MinRange ?? -10_000;
						double maximum = record.NumberParameterOptions.NumberParameterOptions.MaxRange ?? 10_000;
						int decimalVal = Convert.ToInt32(record.NumberParameterOptions.NumberParameterOptions.Decimals);
						double stepSize = record.NumberParameterOptions.NumberParameterOptions.StepSize ?? 1;
						Numeric value = new Numeric(record.ConfigurationParamValue.ConfigurationParameterValue.DoubleValue ?? record.NumberParameterOptions.NumberParameterOptions.DefaultValue ?? 0)
						{
							Minimum = minimum,
							Maximum = maximum,
							StepSize = stepSize,
							Decimals = decimalVal,
						};
						Numeric defaultVal = new Numeric(record.NumberParameterOptions.NumberParameterOptions.DefaultValue ?? 0)
						{
							Minimum = minimum,
							Maximum = maximum,
							StepSize = stepSize,
							Decimals = decimalVal,
						};
						unit.Text = GetDefaultUnit(record.ConfigurationParamValue.ConfigurationParameterValue.NumberValueOptions, parameter.Selected);
						start.Value = minimum;
						start.IsEnabled = true;
						end.Value = maximum;
						end.IsEnabled = true;
						decimals.Value = decimalVal;
						decimals.IsEnabled = true;
						step.Value = stepSize;
						step.IsEnabled = true;

						start.Changed += (sender, args) =>
						{
							value.Minimum = args.Value;
							defaultVal.Minimum = args.Value;
							record.NumberParameterOptions.NumberParameterOptions.MinRange = args.Value;
						};
						end.Changed += (sender, args) =>
						{
							value.Maximum = args.Value;
							defaultVal.Maximum = args.Value;
							record.NumberParameterOptions.NumberParameterOptions.MaxRange = args.Value;
						};
						decimals.Changed += (sender, args) =>
						{
							value.Decimals = Convert.ToInt32(args.Value);
							defaultVal.Decimals = Convert.ToInt32(args.Value);
							record.NumberParameterOptions.NumberParameterOptions.Decimals = Convert.ToInt32(args.Value);
						};
						step.Changed += (sender, args) =>
						{
							value.StepSize = args.Value;
							defaultVal.StepSize = args.Value;
							record.NumberParameterOptions.NumberParameterOptions.StepSize = args.Value;
						};
						value.Changed += (sender, args) => { record.ConfigurationParamValue.ConfigurationParameterValue.DoubleValue = args.Value; };
						defaultVal.Changed += (sender, args) => { record.NumberParameterOptions.NumberParameterOptions.DefaultValue = args.Value; };
						view.AddWidget(value, row, 3);
						view.AddWidget(new WhiteSpace(), row, 9);
						view.AddWidget(defaultVal, row, 10);
					}

						break;

					case SlcConfigurationsIds.Enums.ParameterType.Discrete:
					{
						var discreteOptions = repoConfig.DiscreteParameterOptions.Find(x => x.ID.Id == record.ConfigurationParamValue.ConfigurationParameterValue.DiscreteValueOptions)
							                      ?.DiscreteParameterOptions.DiscreteValues
						                      ?? record.DiscreteParameterOptions?.DiscreteParameterOptions.DiscreteValues
						                      ?? new List<Guid>();
						var discretes = repoConfig.DiscreteValues.Where(x => discreteOptions.Contains(x.ID.Id))
							.Select(x => new Option<DiscreteValuesInstance>(x.DiscreteValue.Value, x))
							.OrderBy(x => x.DisplayValue)
							.ToList();
						var defaultDis = repoConfig.DiscreteParameterOptions.Find(x => x.ID.Id == record.ConfigurationParamValue.ConfigurationParameterValue.DiscreteValueOptions)
							                 ?.DiscreteParameterOptions.DefaultDiscreteValue
						                 ?? record.DiscreteParameterOptions?.DiscreteParameterOptions.DefaultDiscreteValue
						                 ?? Guid.Empty;
						var defaultDiscrete = repoConfig.DiscreteValues.FirstOrDefault(x => x.ID.Id == defaultDis);

						var value = new DropDown<DiscreteValuesInstance>(discretes);
						var defaultVal = new DropDown<DiscreteValuesInstance>(discretes);
						if (record.ConfigurationParamValue.ConfigurationParameterValue.StringValue != null
						    && value.Options.Any(x => x.DisplayValue == record.ConfigurationParamValue.ConfigurationParameterValue.StringValue))
						{
							value.Selected = value.Options.First(x => x.DisplayValue == record.ConfigurationParamValue.ConfigurationParameterValue.StringValue).Value;
						}

						if (defaultVal.Options.Any(x => x.DisplayValue == defaultDiscrete?.Name))
						{
							defaultVal.Selected = value.Options.First(x => x.DisplayValue == defaultDiscrete.Name).Value;
						}

						var values = new Button("...");

						value.Changed += (sender, args) => { record.ConfigurationParamValue.ConfigurationParameterValue.StringValue = args.SelectedOption.DisplayValue; };
						defaultVal.Changed += (sender, args) => { record.DiscreteParameterOptions.DiscreteParameterOptions.DefaultDiscreteValue = args.Selected.ID.Id; };
						values.Pressed += (sender, args) =>
						{
							var optionsView = new DiscreteValuesView(engine);
							optionsView.Options.SetOptions(discretes);
							optionsView.Options.CheckAll();
							optionsView.BtnApply.Pressed += (o, eventArgs) =>
							{
								value.SetOptions(optionsView.Options.CheckedOptions);
								defaultVal.SetOptions(optionsView.Options.CheckedOptions);
								controller.ShowDialog(view);
							};
							controller.ShowDialog(optionsView);
						};
						view.AddWidget(value, row, 3);
						view.AddWidget(values, row, 9);
						view.AddWidget(defaultVal, row, 10);
					}

						break;

					default:
					{
						var value = new TextBox(record.ConfigurationParamValue.ConfigurationParameterValue.StringValue ?? record.TextParameterOptions.TextParameterOptions.Default ?? String.Empty)
						{
							Tooltip = record.TextParameterOptions.TextParameterOptions.UserMessage ?? String.Empty,
						};
						var defaultVal = new TextBox(record.TextParameterOptions.TextParameterOptions.Default ?? String.Empty)
						{
							Tooltip = record.TextParameterOptions.TextParameterOptions.UserMessage ?? String.Empty,
						};
						value.Changed += (sender, args) =>
						{
							if (record.TextParameterOptions.TextParameterOptions.Regex != null && !Regex.IsMatch(args.Value, record.TextParameterOptions.TextParameterOptions.Regex))
							{
								value.ValidationState = UIValidationState.Invalid;
								value.ValidationText = $"Input did not match Regex '{record.TextParameterOptions.TextParameterOptions.Regex}' - reverted to previous value";
								value.Text = args.Previous;
								return;
							}

							value.ValidationState = UIValidationState.Valid;
							value.ValidationText = record.TextParameterOptions.TextParameterOptions.UserMessage;
							record.ConfigurationParamValue.ConfigurationParameterValue.StringValue = args.Value;
						};
						defaultVal.Changed += (sender, args) => { record.TextParameterOptions.TextParameterOptions.Default = args.Value; };
						view.AddWidget(value, row, 3);
						view.AddWidget(new WhiteSpace(), row, 9);
						view.AddWidget(defaultVal, row, 10);
					}

						break;
				}
			}

			// Populate row
			view.AddWidget(label, row, 0);
			view.AddWidget(parameter, row, 1);
			view.AddWidget(link, row, 2);
			view.AddWidget(unit, row, 4);
			view.AddWidget(start, row, 5);
			view.AddWidget(end, row, 6);
			view.AddWidget(step, row, 7);
			view.AddWidget(decimals, row, 8);
			view.AddWidget(exposeAtOrder, row, 11);
			view.AddWidget(mandatoryAtOrder, row, 12);
			view.AddWidget(mandatoryAtService, row, 13);
			view.AddWidget(delete, row, 14);
		}

		private List<ServiceSpecificationConfigurationValueInstance> GetCurrentConfigurationParameters()
		{
			if (instance.ServiceSpecificationInfo.ServiceSpecificationConfigurationParameters.Any())
			{
				FilterElement<DomInstance> filter = new ORFilterElement<DomInstance>();
				foreach (Guid configurationParameter in instance.ServiceSpecificationInfo.ServiceSpecificationConfigurationParameters)
				{
					filter = filter.OR(DomInstanceExposers.Id.Equal(configurationParameter));
				}

				engine.GenerateInformation(String.Join(";", domHelperSrvMgmt.DomInstances.Read(filter).Select(x => x.Name)));

				return domHelperSrvMgmt.DomInstances.Read(filter).Select(x => new ServiceSpecificationConfigurationValueInstance(x)).ToList();
			}

			return new List<ServiceSpecificationConfigurationValueInstance>();
		}

		private string GetDefaultUnit(Guid? numberValueOptions, ConfigurationParametersInstance parameter)
		{
			Guid? refId = default(Guid?);
			refId = GetNumberOptions(numberValueOptions, parameter)?.NumberParameterOptions.DefaultUnit;

			if (!refId.HasValue)
			{
				return "-";
			}

			return repoConfig.ConfigurationUnits.Find(x => x.ID.Id == refId.Value)?.ConfigurationUnitInfo.UnitName ?? "-";
		}

		private NumberParameterOptionsInstance GetNumberOptions(Guid? numberValueOptions, ConfigurationParametersInstance parameter)
		{
			return numberValueOptions.HasValue
				? repoConfig.NumberParameterOptions.FirstOrDefault(x => x.ID.Id == numberValueOptions.Value)
				: repoConfig.NumberParameterOptions.FirstOrDefault(x => x.ID.Id == parameter.ConfigurationParameterInfo.NumberOptions);
		}

		private sealed class DataRecord
		{
			public State State { get; set; }

			public ServiceSpecificationConfigurationValueInstance ServiceConfig { get; set; }

			public ConfigurationParameterValueInstance ConfigurationParamValue { get; set; }

			public ConfigurationParametersInstance ConfigurationParam { get; set; }

			public NumberParameterOptionsInstance NumberParameterOptions { get; set; }

			public DiscreteParameterOptionsInstance DiscreteParameterOptions { get; set; }

			public TextParameterOptionsInstance TextParameterOptions { get; set; }
		}
	}
}