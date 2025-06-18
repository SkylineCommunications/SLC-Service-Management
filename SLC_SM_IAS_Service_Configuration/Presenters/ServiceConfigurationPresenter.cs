namespace SLC_SM_IAS_Service_Configuration.Presenters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices.WindowsRuntime;
	using System.Text.RegularExpressions;

	using DomHelpers.SlcConfigurations;

	using Library;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	using SLC_SM_Common.API.ServiceManagementApi;

	using SLC_SM_IAS_Service_Configuration.Views;

	public class ServiceConfigurationPresenter
	{
		private readonly List<DataRecord> configurations = new List<DataRecord>();
		private readonly IEngine engine;
		private readonly InteractiveController controller;
		private readonly Models.Service instance;
		private readonly ServiceConfigurationView view;
		private RepoConfigurations repoConfig;
		private Repo repoService;

		public ServiceConfigurationPresenter(IEngine engine, InteractiveController controller, ServiceConfigurationView view, Models.Service instance)
		{
			this.engine = engine;
			this.controller = controller;
			this.view = view;
			this.instance = instance;

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
			repoService = new Repo(Engine.SLNetRaw);
			repoConfig = new RepoConfigurations(Engine.SLNetRaw);

			var configParams = repoConfig.ConfigurationParameters.Read();

			if (instance.Configurations != null)
			{
				foreach (var currentConfig in instance.Configurations)
				{
					var configParam = configParams.Find(x => x.ID == currentConfig.ConfigurationParameter.ConfigurationParameterId);
					if (configParam == null)
					{
						continue;
					}

					DataRecord dataRecord = BuildDataRecord(currentConfig, configParam);
					configurations.Add(dataRecord);
				}
			}

			BuildUI();
		}

		public void StoreModels()
		{
			foreach (var configuration in configurations)
			{
				if (configuration.State == State.Delete)
				{
					repoService.ServiceConfigurationValues.TryDelete(configuration.ServiceConfig);
				}
				else
				{
					repoService.ServiceConfigurationValues.CreateOrUpdate(configuration.ServiceConfig);
					repoConfig.ConfigurationParameterValues.CreateOrUpdate(configuration.ConfigurationParamValue);
				}
			}

			repoService.Services.CreateOrUpdate(instance);
		}

		private void AddConfigModel(SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameter selectedParameter)
		{
			var configurationParameterInstance = selectedParameter ?? new SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameter();
			var config = new Models.ServiceConfigurationValue
			{
				ID = Guid.NewGuid(),
				Mandatory = false,
				ConfigurationParameter = new SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameterValue
				{
					Label = String.Empty,
					Type = configurationParameterInstance.Type,
					ConfigurationParameterId = configurationParameterInstance.ID,
					NumberOptions = configurationParameterInstance.NumberOptions,
					DiscreteOptions = configurationParameterInstance.DiscreteOptions,
					TextOptions = configurationParameterInstance.TextOptions,
				},
			};
			if (config.ConfigurationParameter.NumberOptions != null)
			{
				config.ConfigurationParameter.NumberOptions.ID = Guid.NewGuid();
			}

			if (config.ConfigurationParameter.DiscreteOptions != null)
			{
				config.ConfigurationParameter.DiscreteOptions.ID = Guid.NewGuid();
			}

			if (config.ConfigurationParameter.TextOptions != null)
			{
				config.ConfigurationParameter.TextOptions.ID = Guid.NewGuid();
			}

			instance.Configurations.Add(config);

			configurations.Add(BuildDataRecord(config, configurationParameterInstance));
		}

		private DataRecord BuildDataRecord(Models.ServiceConfigurationValue currentConfig, SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameter configParam)
		{
			var dataRecord = new DataRecord
			{
				State = State.Update,
				ServiceConfig = currentConfig,
				ConfigurationParamValue = currentConfig.ConfigurationParameter,
				ConfigurationParam = configParam,
			};
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
			var lblMandatoryAtService = new Label("Mandatory");

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
			view.AddWidget(lblMandatoryAtService, row, 10);
		}

		private void BuildUI()
		{
			view.Clear();

			int row = 0;
			view.AddWidget(view.TitleDetails, row, 0, 1, 2);

			BuildHeaderRow(++row);

			foreach (var configuration in configurations.Where(x => x.State != State.Delete))
			{
				BuildUIRow(configuration, ++row);
			}

			view.AddWidget(new WhiteSpace(), ++row, 0);
			var parameterOptions = repoConfig.ConfigurationParameters.Read().Select(x => new Option<SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameter>(x.Name, x)).OrderBy(x => x.DisplayValue).ToList();
			parameterOptions.Insert(0, new Option<SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameter>("- Add -", null));
			var parameter = new DropDown<SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameter>(parameterOptions);
			view.AddWidget(parameter, ++row, 1);
			parameter.Changed += (sender, args) =>
			{
				if (args.Selected == null)
				{
					return;
				}

				AddConfigModel(args.Selected);
				BuildUI();
			};

			view.AddWidget(new WhiteSpace(), ++row, 0);
			view.AddWidget(view.BtnCancel, ++row, 0);
			view.AddWidget(view.BtnUpdate, row, 1);
		}

		private void BuildUIRow(DataRecord record, int row)
		{
			// Init
			var label = new TextBox(record.ConfigurationParamValue.Label) { MaxWidth = 150 };
			var parameter = new DropDown<SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameter>(
				new[] { new Option<SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameter>(record.ConfigurationParam.Name, record.ConfigurationParam) })
			{
				IsEnabled = false,
				MaxWidth = 200,
			};
			var link = new CheckBox { IsChecked = record.ConfigurationParamValue.LinkedConfigurationReference != null };
			var unit = new DropDown<SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationUnit>(new[] { new Option<SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationUnit>("-", null) }) { IsEnabled = false, MaxWidth = 80 };
			var start = new Numeric { IsEnabled = false, MaxWidth = 100 };
			var end = new Numeric { IsEnabled = false, MaxWidth = 100 };
			var step = new Numeric { IsEnabled = false, Minimum = 0, Maximum = 1, MaxWidth = 100 };
			var decimals = new Numeric { StepSize = 1, Minimum = 0, Maximum = 6, IsEnabled = false, MaxWidth = 80 };
			var values = new Button("...") { IsEnabled = false };
			var mandatoryAtService = new CheckBox { IsChecked = record.ServiceConfig.Mandatory, IsEnabled = false };
			var delete = new Button("🚫") { IsEnabled = !record.ServiceConfig.Mandatory };

			label.Changed += (sender, args) => record.ConfigurationParamValue.Label = args.Value;
			delete.Pressed += (sender, args) =>
			{
				record.State = State.Delete;
				instance.Configurations.Remove(record.ServiceConfig);
				BuildUI();
			};
			link.Changed += (sender, args) =>
			{
				record.ConfigurationParamValue.LinkedConfigurationReference = args.IsChecked ? "Dummy Link" : null;
				BuildUI();
			};

			if (record.ConfigurationParamValue.LinkedConfigurationReference != null)
			{
				view.AddWidget(new DropDown(), row, 3);
				view.AddWidget(new WhiteSpace(), row, 10);
			}
			else
			{
				switch (parameter.Selected.Type)
				{
					case SlcConfigurationsIds.Enums.Type.Number:
						{
							double minimum = record.ConfigurationParamValue.NumberOptions.MinRange ?? -10_000;
							double maximum = record.ConfigurationParamValue.NumberOptions.MaxRange ?? 10_000;
							int decimalVal = Convert.ToInt32(record.ConfigurationParamValue.NumberOptions.Decimals);
							double stepSize = record.ConfigurationParamValue.NumberOptions.StepSize ?? 1;
							Numeric value = new Numeric(record.ConfigurationParamValue.DoubleValue ?? record.ConfigurationParamValue.NumberOptions.DefaultValue ?? 0)
							{
								Minimum = minimum,
								Maximum = maximum,
								StepSize = stepSize,
								Decimals = decimalVal,
								MaxWidth = 150,
							};
							unit.SetOptions(GetUnits(record.ConfigurationParamValue.NumberOptions, parameter.Selected));
							unit.Selected = GetDefaultUnit(record.ConfigurationParamValue.NumberOptions, parameter.Selected);
							unit.IsEnabled = true;
							start.Value = minimum;
							start.IsEnabled = true;
							end.Value = maximum;
							end.IsEnabled = true;
							decimals.Value = decimalVal;
							decimals.IsEnabled = true;
							step.Value = stepSize;
							step.StepSize = 1 / Math.Pow(10, decimalVal);
							step.Decimals = decimalVal;
							step.IsEnabled = true;

							start.Changed += (sender, args) =>
							{
								value.Minimum = args.Value;
								step.Minimum = args.Value;
								record.ConfigurationParamValue.NumberOptions.MinRange = args.Value;
							};
							end.Changed += (sender, args) =>
							{
								value.Maximum = args.Value;
								step.Maximum = args.Value;
								record.ConfigurationParamValue.NumberOptions.MaxRange = args.Value;
							};
							decimals.Changed += (sender, args) =>
							{
								value.Decimals = Convert.ToInt32(args.Value);
								step.Decimals = Convert.ToInt32(args.Value);
								double newStepsize = 1 / Math.Pow(10, args.Value);
								value.StepSize = newStepsize;
								step.StepSize = newStepsize;
								record.ConfigurationParamValue.NumberOptions.Decimals = Convert.ToInt32(args.Value);
							};
							step.Changed += (sender, args) =>
							{
								value.StepSize = args.Value;
								record.ConfigurationParamValue.NumberOptions.StepSize = args.Value;
							};
							unit.Changed += (sender, args) => record.ConfigurationParamValue.NumberOptions.DefaultUnit = args.Selected;
							value.Changed += (sender, args) => { record.ConfigurationParamValue.DoubleValue = args.Value; };
							view.AddWidget(value, row, 3);
						}

						break;

					case SlcConfigurationsIds.Enums.Type.Discrete:
						{
							var discretes = record.ConfigurationParamValue.DiscreteOptions.DiscreteValues
								.Select(x => new Option<SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue>(x.Value, x))
								.OrderBy(x => x.DisplayValue)
								.ToList();

							var value = new DropDown<SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue>(discretes) { MaxWidth = 150 };
							if (record.ConfigurationParamValue.StringValue != null
								&& value.Options.Any(x => x.DisplayValue == record.ConfigurationParamValue.StringValue))
							{
								value.Selected = value.Options.First(x => x.DisplayValue == record.ConfigurationParamValue.StringValue).Value;
							}

							values.IsEnabled = true;

							value.Changed += (sender, args) => { record.ConfigurationParamValue.StringValue = args.SelectedOption.DisplayValue; };
							values.Pressed += (sender, args) =>
							{
								var optionsView = new DiscreteValuesView(engine);
								optionsView.Options.SetOptions(discretes);
								optionsView.Options.CheckAll();
								optionsView.BtnApply.Pressed += (o, eventArgs) =>
								{
									value.SetOptions(optionsView.Options.CheckedOptions);
									controller.ShowDialog(view);
								};
								controller.ShowDialog(optionsView);
							};
							view.AddWidget(value, row, 3);
						}

						break;

					default:
						{
							var value = new TextBox(record.ConfigurationParamValue.StringValue ?? record.ConfigurationParamValue.TextOptions?.Default ?? String.Empty)
							{
								Tooltip = record.ConfigurationParamValue.TextOptions?.UserMessage ?? String.Empty,
								MaxWidth = 150,
							};
							value.Changed += (sender, args) =>
							{
								if (record.ConfigurationParamValue.TextOptions?.Regex != null && !Regex.IsMatch(args.Value, record.ConfigurationParamValue.TextOptions.Regex))
								{
									value.ValidationState = UIValidationState.Invalid;
									value.ValidationText = $"Input did not match Regex '{record.ConfigurationParamValue.TextOptions.Regex}' - reverted to previous value";
									value.Text = args.Previous;
									return;
								}

								value.ValidationState = UIValidationState.Valid;
								value.ValidationText = record.ConfigurationParamValue.TextOptions?.UserMessage;
								record.ConfigurationParamValue.StringValue = args.Value;
							};
							view.AddWidget(value, row, 3);
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
			view.AddWidget(values, row, 9);
			view.AddWidget(mandatoryAtService, row, 10);
			view.AddWidget(delete, row, 11);
		}

		private List<Option<SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationUnit>> GetUnits(SLC_SM_Common.API.ConfigurationsApi.Models.NumberParameterOptions numberValueOptions, SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameter parameter)
		{
			var units = new List<Option<SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationUnit>>();
			if (numberValueOptions?.DefaultUnit != null)
			{
				units.AddRange(numberValueOptions.Units.Select(x => new Option<SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationUnit>(x.Name, x)));
			}
			else if (parameter.NumberOptions?.DefaultUnit != null)
			{
				units.AddRange(parameter.NumberOptions.Units.Select(x => new Option<SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationUnit>(x.Name, x)));
			}

			units = units.OrderBy(x => x.DisplayValue).ToList();

			units.Insert(0, new Option<SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationUnit>("-", null));
			return units;
		}

		private SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationUnit GetDefaultUnit(SLC_SM_Common.API.ConfigurationsApi.Models.NumberParameterOptions numberValueOptions, SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameter parameter)
		{
			if (numberValueOptions != null)
			{
				return numberValueOptions.DefaultUnit;
			}

			if (parameter.NumberOptions != null)
			{
				return parameter.NumberOptions.DefaultUnit;
			}

			return null;
		}

		private sealed class DataRecord
		{
			public State State { get; set; }

			public Models.ServiceConfigurationValue ServiceConfig { get; set; }

			public SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameterValue ConfigurationParamValue { get; set; }

			public SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameter ConfigurationParam { get; set; }
		}
	}
}