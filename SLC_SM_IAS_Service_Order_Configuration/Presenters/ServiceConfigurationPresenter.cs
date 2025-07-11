﻿namespace SLC_SM_IAS_Service_Order_Configuration.Presenters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;

	using DomHelpers.SlcConfigurations;

	using Library;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	using SLC_SM_Common.API.ServiceManagementApi;

	using SLC_SM_IAS_Service_Order_Configuration.Views;

	public class ServiceConfigurationPresenter
	{
		private readonly List<DataRecord> configurations = new List<DataRecord>();
		private readonly IEngine engine;
		private readonly InteractiveController controller;
		private readonly Models.ServiceOrderItem instance;
		private readonly ServiceConfigurationView view;
		private RepoConfigurations repoConfig;
		private Repo repoService;

		public ServiceConfigurationPresenter(IEngine engine, InteractiveController controller, ServiceConfigurationView view, Models.ServiceOrderItem instance)
		{
			this.engine = engine;
			this.controller = controller;
			this.view = view;
			this.instance = instance;

			view.BtnCancel.Pressed += (sender, args) => throw new ScriptAbortException("OK");
			view.BtnShowValueDetails.Pressed += (sender, args) =>
			{
				view.BtnShowValueDetails.Text = view.Details.IsVisible ? view.BtnShowValueDetails.Text.Replace("Hide", "Show") : view.BtnShowValueDetails.Text.Replace("Show", "Hide");
				view.Details.IsVisible = !view.Details.IsVisible;
			};
			view.BtnShowLifeCycleDetails.Pressed += (sender, args) =>
			{
				view.BtnShowLifeCycleDetails.Text = view.LifeCycleDetails.IsVisible ? view.BtnShowLifeCycleDetails.Text.Replace("Hide", "Show") : view.BtnShowLifeCycleDetails.Text.Replace("Show", "Hide");
				view.LifeCycleDetails.IsVisible = !view.LifeCycleDetails.IsVisible;
			};
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
					var configParam = configParams.Find(x => x.ID == currentConfig?.ConfigurationParameter?.ConfigurationParameterId);
					if (configParam == null)
					{
						continue;
					}

					DataRecord dataRecord = BuildDataRecord(currentConfig, configParam);
					configurations.Add(dataRecord);
				}
			}

			BuildUI(false, false);
		}

		public void StoreModels()
		{
			foreach (var configuration in configurations)
			{
				if (configuration.State == State.Delete)
				{
					repoService.ServiceOrderItemConfigurationValues.TryDelete(configuration.ServiceConfig);
				}
			}

			repoService.ServiceOrderItems.CreateOrUpdate(instance);
		}

		private void AddConfigModel(SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameter selectedParameter)
		{
			var configurationParameterInstance = selectedParameter ?? new SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameter();
			var config = new Models.ServiceOrderItemConfigurationValue
			{
				ID = Guid.NewGuid(),
				Mandatory = true,
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

		private DataRecord BuildDataRecord(Models.ServiceOrderItemConfigurationValue currentConfig, SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameter configParam)
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
			var lblDefault = new Label("Fixed");
			var lblMandatoryAtService = new Label("Mandatory");

			view.AddWidget(lblLabel, row, 0);
			view.AddWidget(lblParameter, row, 1);
			view.AddWidget(lblLink, row, 2);
			view.AddWidget(lblValue, row, 3);
			view.AddWidget(lblUnit, row, 4);

			view.Details.AddWidget(lblStart, 0, 0);
			view.Details.AddWidget(lblEnd, 0, 1);
			view.Details.AddWidget(lblStop, 0, 2);
			view.Details.AddWidget(lblDecimals, 0, 3);
			view.Details.AddWidget(lblValues, 0, 4);
			view.LifeCycleDetails.AddWidget(lblDefault, 0, 0);
			view.LifeCycleDetails.AddWidget(lblMandatoryAtService, 0, 1);
		}

		private void BuildUI(bool showDetails, bool showLifeCycleDetails)
		{
			view.Clear();

			int row = 0;
			view.AddWidget(view.TitleDetails, row, 0, 1, 2);
			view.AddWidget(new WhiteSpace(), ++row, 0);
			view.AddWidget(view.BtnShowValueDetails, ++row, 0);
			view.AddWidget(view.BtnShowLifeCycleDetails, row, 1);
			view.AddWidget(new WhiteSpace(), ++row, 0);

			BuildHeaderRow(++row);

			int originalSectionRow = row;
			int sectionRow = 0;
			foreach (var configuration in configurations.Where(x => x.State != State.Delete))
			{
				BuildUIRow(configuration, ++row, ++sectionRow);
			}

			view.AddSection(view.Details, originalSectionRow, 5);
			view.AddSection(view.LifeCycleDetails, originalSectionRow, 10);
			view.Details.IsVisible = showDetails;
			view.LifeCycleDetails.IsVisible = showLifeCycleDetails;

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
				BuildUI(view.Details.IsVisible, view.LifeCycleDetails.IsVisible);
			};

			view.AddWidget(new WhiteSpace(), ++row, 0);
			view.AddWidget(view.BtnCancel, ++row, 0);
			view.AddWidget(view.BtnUpdate, row, 1);
		}

		private void BuildUIRow(DataRecord record, int row, int sectionRow)
		{
			// Init
			var label = new TextBox(record.ConfigurationParamValue.Label);
			var parameter = new DropDown<SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameter>(
				new[] { new Option<SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameter>(record.ConfigurationParam.Name, record.ConfigurationParam) })
			{
				IsEnabled = false,
			};
			var isFixed = new CheckBox { IsChecked = record.ConfigurationParamValue.ValueFixed, IsEnabled = false };
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
			isFixed.Changed += (sender, args) => record.ConfigurationParamValue.ValueFixed = args.IsChecked;
			delete.Pressed += (sender, args) =>
			{
				record.State = State.Delete;
				instance.Configurations.Remove(record.ServiceConfig);
				BuildUI(view.Details.IsVisible, view.LifeCycleDetails.IsVisible);
			};
			link.Changed += (sender, args) =>
			{
				record.ConfigurationParamValue.LinkedConfigurationReference = args.IsChecked ? "Dummy Link" : null;
				BuildUI(view.Details.IsVisible, view.LifeCycleDetails.IsVisible);
			};

			if (record.ConfigurationParamValue.LinkedConfigurationReference != null)
			{
				view.AddWidget(new DropDown(), row, 3);
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
								IsEnabled = !isFixed.IsChecked,
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

							var value = new DropDown<SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue>(discretes) { IsEnabled = !isFixed.IsChecked };
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
								IsEnabled = !isFixed.IsChecked,
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

			view.Details.AddWidget(start, sectionRow, 0);
			view.Details.AddWidget(end, sectionRow, 1);
			view.Details.AddWidget(step, sectionRow, 2);
			view.Details.AddWidget(decimals, sectionRow, 3);
			view.Details.AddWidget(values, sectionRow, 4);
			view.LifeCycleDetails.AddWidget(isFixed, sectionRow, 0);
			view.LifeCycleDetails.AddWidget(mandatoryAtService, sectionRow, 1);

			view.AddWidget(delete, row, 12);
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

			public Models.ServiceOrderItemConfigurationValue ServiceConfig { get; set; }

			public SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameterValue ConfigurationParamValue { get; set; }

			public SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameter ConfigurationParam { get; set; }
		}
	}
}