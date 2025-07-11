namespace SLC_SM_IAS_Configurations.Presenters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;

	using DomHelpers.SlcConfigurations;

	using Library;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	using SLC_SM_Common.API.ConfigurationsApi;

	using SLC_SM_IAS_Configurations.Views;

	public class ConfigurationPresenter
	{
		private readonly List<DataRecord> configurations = new List<DataRecord>();
		private readonly InteractiveController controller;
		private readonly IEngine engine;
		private readonly ConfigurationView view;
		private List<Option<Models.ConfigurationUnit>> cachedUnits;
		private RepoConfigurations repoConfig;

		public ConfigurationPresenter(IEngine engine, InteractiveController controller, ConfigurationView view)
		{
			this.engine = engine;
			this.controller = controller;
			this.view = view;

			view.BtnCancel.Pressed += (sender, args) => throw new ScriptAbortException("OK");
			view.BtnUpdate.Pressed += (sender, args) =>
			{
				StoreModels();
				throw new ScriptAbortException("OK");
			};
		}

		private enum State
		{
			Equal,
			Update,
			Delete,
		}

		public void LoadFromModel()
		{
			repoConfig = new RepoConfigurations(Engine.SLNetRaw);
			cachedUnits = repoConfig.ConfigurationUnits.Read().Select(x => new Option<Models.ConfigurationUnit>(x.Name, x)).OrderBy(x => x.DisplayValue).ToList();
			cachedUnits.Insert(0, new Option<Models.ConfigurationUnit>("-", null));

			var configParams = repoConfig.ConfigurationParameters.Read();

			foreach (var configurationParameter in configParams)
			{
				DataRecord dataRecord = BuildDataRecord(configurationParameter, State.Equal);
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
					repoConfig.ConfigurationParameters.TryDelete(configuration.ConfigurationParam);
				}
				else if (configuration.State == State.Update)
				{
					repoConfig.ConfigurationParameters.CreateOrUpdate(configuration.ConfigurationParam);
				}
				else
				{
					// nothing to do
				}
			}
		}

		private static bool ValidateTextValue(DataRecord record, string newValue, TextBox value)
		{
			if (record.ConfigurationParam.TextOptions.Regex != null && !Regex.IsMatch(newValue, record.ConfigurationParam.TextOptions.Regex))
			{
				value.ValidationState = UIValidationState.Invalid;
				value.ValidationText = $"Input did not match Regex '{record.ConfigurationParam.TextOptions.Regex}' - reverted to previous value";
				value.Text = String.Empty;
				return true;
			}

			value.ValidationState = UIValidationState.Valid;
			value.ValidationText = record.ConfigurationParam.TextOptions.UserMessage;
			return true;
		}

		private void AddConfigModel()
		{
			var configurationParameterInstance = new Models.ConfigurationParameter
			{
				Name = $"Parameter #{configurations.Count + 1:000}",
			};
			configurations.Add(BuildDataRecord(configurationParameterInstance, State.Update));
		}

		private DataRecord BuildDataRecord(Models.ConfigurationParameter configParam, State initialState)
		{
			State state = initialState;
			switch (configParam.Type)
			{
				case SlcConfigurationsIds.Enums.Type.Number:
					if (configParam.NumberOptions == null)
					{
						configParam.NumberOptions = new Models.NumberParameterOptions();
						state = State.Update;
					}

					break;

				case SlcConfigurationsIds.Enums.Type.Discrete:
					if (configParam.DiscreteOptions == null)
					{
						configParam.DiscreteOptions = new Models.DiscreteParameterOptions();
						state = State.Update;
					}

					break;

				case SlcConfigurationsIds.Enums.Type.Text:
					if (configParam.TextOptions == null)
					{
						configParam.TextOptions = new Models.TextParameterOptions();
						state = State.Update;
					}

					break;

				default:
					// Nothing to do
					break;
			}

			var dataRecord = new DataRecord
			{
				State = state,
				ConfigurationParam = configParam,
			};
			return dataRecord;
		}

		private void BuildHeaderRow(int row)
		{
			var lblName = new Label("Name");
			var lblType = new Label("Type");
			var lblUnit = new Label("Unit");
			var lblStart = new Label("Start");
			var lblEnd = new Label("End");
			var lblStop = new Label("Step Size");
			var lblDecimals = new Label("Decimals");
			var lblValues = new Label("Values");
			var lblDefault = new Label("Default Value");

			view.AddWidget(lblName, row, 0);
			view.AddWidget(lblType, row, 1);
			view.AddWidget(lblUnit, row, 2);
			view.AddWidget(lblStart, row, 3);
			view.AddWidget(lblEnd, row, 4);
			view.AddWidget(lblStop, row, 5);
			view.AddWidget(lblDecimals, row, 6);
			view.AddWidget(lblValues, row, 7);
			view.AddWidget(lblDefault, row, 8);
		}

		private void BuildUI()
		{
			view.Clear();

			int row = 0;
			BuildHeaderRow(++row);

			foreach (var configuration in configurations.Where(x => x.State != State.Delete))
			{
				BuildUIRow(configuration, ++row);
			}

			var parameter = new Button("➕");
			view.AddWidget(parameter, ++row, 0);
			parameter.Pressed += (sender, args) =>
			{
				AddConfigModel();
				BuildUI();
			};

			view.AddWidget(new WhiteSpace(), ++row, 0);
			view.AddWidget(view.BtnCancel, ++row, 0);
			view.AddWidget(view.BtnUpdate, row, 1);
		}

		private void BuildUIRow(DataRecord record, int row)
		{
			// Init
			var label = new TextBox(record.ConfigurationParam.Name) { MinWidth = 120 };
			var paramType = new EnumDropDown<SlcConfigurationsIds.Enums.Type> { Selected = record.ConfigurationParam.Type };
			var unit = new DropDown<Models.ConfigurationUnit>(cachedUnits) { IsEnabled = false, MaxWidth = 80 };
			var start = new Numeric { IsEnabled = false, MaxWidth = 100 };
			var end = new Numeric { IsEnabled = false, MaxWidth = 100 };
			var step = new Numeric { IsEnabled = false, Minimum = 0, Maximum = 1, MaxWidth = 100 };
			var decimals = new Numeric { StepSize = 1, Minimum = 0, Maximum = 6, IsEnabled = false, MaxWidth = 80 };
			var values = new Button("...") { IsEnabled = false };
			var delete = new Button("🚫");

			label.Changed += (sender, args) =>
			{
				if (String.IsNullOrEmpty(args.Value))
				{
					label.ValidationState = UIValidationState.Invalid;
					label.ValidationText = "A name must be provided";
					label.Text = args.Previous;
					return;
				}

				label.ValidationState = UIValidationState.Valid;
				label.ValidationText = String.Empty;

				record.ConfigurationParam.Name = args.Value;
				record.State = State.Update;
			};
			delete.Pressed += (sender, args) =>
			{
				record.State = State.Delete;
				BuildUI();
			};
			paramType.Changed += (sender, args) =>
			{
				record.ConfigurationParam.Type = args.Selected;
				switch (args.Selected)
				{
					case SlcConfigurationsIds.Enums.Type.Number:
						if (record.ConfigurationParam.NumberOptions == null)
						{
							record.ConfigurationParam.NumberOptions = new Models.NumberParameterOptions();
						}

						break;

					case SlcConfigurationsIds.Enums.Type.Discrete:
						if (record.ConfigurationParam.DiscreteOptions == null)
						{
							record.ConfigurationParam.DiscreteOptions = new Models.DiscreteParameterOptions();
						}

						break;

					case SlcConfigurationsIds.Enums.Type.Text:
						if (record.ConfigurationParam.TextOptions == null)
						{
							record.ConfigurationParam.TextOptions = new Models.TextParameterOptions();
						}

						break;

					default:
						// Nothing to do
						break;
				}

				BuildUI();
			};

			if (String.IsNullOrEmpty(label.Text))
			{
				label.ValidationState = UIValidationState.Invalid;
				label.ValidationText = "A name must be provided";
			}

			switch (record.ConfigurationParam.Type)
			{
				case SlcConfigurationsIds.Enums.Type.Number:
					{
						double minimum = record.ConfigurationParam.NumberOptions.MinRange ?? -10_000;
						double maximum = record.ConfigurationParam.NumberOptions.MaxRange ?? 10_000;
						int decimalVal = Convert.ToInt32(record.ConfigurationParam.NumberOptions.Decimals);
						double stepSize = record.ConfigurationParam.NumberOptions.StepSize ?? 1;
						Numeric value = new Numeric(record.ConfigurationParam.NumberOptions.DefaultValue ?? 0)
						{
							Minimum = minimum,
							Maximum = maximum,
							StepSize = stepSize,
							Decimals = decimalVal,
						};
						unit.Selected = unit.Options.FirstOrDefault(x => x?.DisplayValue == record.ConfigurationParam.NumberOptions.DefaultUnit?.Name)?.Value;
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
							record.ConfigurationParam.NumberOptions.MinRange = args.Value;
							record.State = State.Update;
						};
						end.Changed += (sender, args) =>
						{
							value.Maximum = args.Value;
							step.Maximum = args.Value;
							record.ConfigurationParam.NumberOptions.MaxRange = args.Value;
							record.State = State.Update;
						};
						decimals.Changed += (sender, args) =>
						{
							value.Decimals = Convert.ToInt32(args.Value);
							step.Decimals = Convert.ToInt32(args.Value);
							double newStepsize = 1 / Math.Pow(10, args.Value);
							value.StepSize = newStepsize;
							step.StepSize = newStepsize;
							record.ConfigurationParam.NumberOptions.Decimals = Convert.ToInt32(args.Value);
							record.State = State.Update;
						};
						step.Changed += (sender, args) =>
						{
							value.StepSize = args.Value;
							record.ConfigurationParam.NumberOptions.StepSize = args.Value;
							record.State = State.Update;
						};
						unit.Changed += (sender, args) =>
						{
							record.ConfigurationParam.NumberOptions.DefaultUnit = args.Selected;
							record.State = State.Update;
						};
						value.Changed += (sender, args) =>
						{
							record.ConfigurationParam.NumberOptions.DefaultValue = args.Value;
							record.State = State.Update;
						};
						view.AddWidget(value, row, 8);
					}

					break;

				case SlcConfigurationsIds.Enums.Type.Discrete:
					{
						var checkedDiscretes = record.ConfigurationParam.DiscreteOptions.DiscreteValues
							.Select(x => new Option<Models.DiscreteValue>(x.Value, x))
							.OrderBy(x => x.DisplayValue)
							.ToList();

						var value = new DropDown<Models.DiscreteValue>(checkedDiscretes);
						if (record.ConfigurationParam.DiscreteOptions.Default != null
							&& value.Options.Any(x => x.DisplayValue == record.ConfigurationParam.DiscreteOptions.Default.Value))
						{
							value.Selected = value.Options.First(x => x.DisplayValue == record.ConfigurationParam.DiscreteOptions.Default.Value).Value;
						}

						values.IsEnabled = true;

						value.Changed += (sender, args) =>
						{
							record.ConfigurationParam.DiscreteOptions.Default = args.Selected;
							record.State = State.Update;
						};
						values.Pressed += (sender, args) =>
						{
							var optionsView = new DiscreteValuesView(engine);
							optionsView.Options.SetOptions(repoConfig.DiscreteValues.Read().Select(x => new Option<Models.DiscreteValue>(x.Value, x)).OrderBy(x => x.DisplayValue));
							foreach (var discrete in optionsView.Options.Options.ToList())
							{
								if (checkedDiscretes.Exists(d => d.DisplayValue == discrete.DisplayValue))
								{
									optionsView.Options.Check(discrete);
								}
							}

							optionsView.BtnReturn.Pressed += (o, eventArgs) => controller.ShowDialog(view);
							optionsView.BtnApply.Pressed += (o, eventArgs) =>
							{
								record.ConfigurationParam.DiscreteOptions.DiscreteValues = optionsView.Options.Checked.ToList();
								value.SetOptions(optionsView.Options.CheckedOptions);
								record.State = State.Update;
								controller.ShowDialog(view);
							};
							controller.ShowDialog(optionsView);
						};
						view.AddWidget(value, row, 8);
					}

					break;

				default:
					{
						var value = new TextBox(record.ConfigurationParam.TextOptions.Default ?? String.Empty)
						{
							Tooltip = record.ConfigurationParam.TextOptions.UserMessage ?? String.Empty,
						};

						values.IsEnabled = true;

						value.Changed += (sender, args) =>
						{
							if (!ValidateTextValue(record, args.Value, value))
							{
								return;
							}

							record.ConfigurationParam.TextOptions.Default = args.Value;

							record.State = State.Update;
						};
						values.Pressed += (sender, args) =>
						{
							var optionsView = new TextOptionsView(engine);
							optionsView.Regex.Text = record.ConfigurationParam.TextOptions.Regex;
							optionsView.UserMessage.Text = record.ConfigurationParam.TextOptions.UserMessage;

							optionsView.BtnReturn.Pressed += (o, eventArgs) => controller.ShowDialog(view);
							optionsView.BtnApply.Pressed += (o, eventArgs) =>
							{
								record.ConfigurationParam.TextOptions.Regex = optionsView.Regex.Text;
								record.ConfigurationParam.TextOptions.UserMessage = optionsView.UserMessage.Text;
								ValidateTextValue(record, value.Text, value);

								record.State = State.Update;
								controller.ShowDialog(view);
							};
							controller.ShowDialog(optionsView);
						};
						view.AddWidget(value, row, 8);
					}

					break;
			}

			// Populate row
			view.AddWidget(label, row, 0);
			view.AddWidget(paramType, row, 1);
			view.AddWidget(unit, row, 2);

			view.AddWidget(start, row, 3);
			view.AddWidget(end, row, 4);
			view.AddWidget(step, row, 5);
			view.AddWidget(decimals, row, 6);
			view.AddWidget(values, row, 7);

			view.AddWidget(delete, row, 9);
		}

		private sealed class DataRecord
		{
			public State State { get; set; }

			public Models.ConfigurationParameter ConfigurationParam { get; set; }
		}
	}
}