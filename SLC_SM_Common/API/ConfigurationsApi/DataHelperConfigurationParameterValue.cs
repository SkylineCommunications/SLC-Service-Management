namespace SLC_SM_Common.API.ConfigurationsApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcConfigurations;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class DataHelperConfigurationParameterValue : DataHelper<Models.ConfigurationParameterValue>
	{
		public DataHelperConfigurationParameterValue(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.ConfigurationParameterValue)
		{
		}

		public override List<Models.ConfigurationParameterValue> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new ConfigurationParameterValueInstance(x))
				.ToList();

			var numberOptions = new DataHelperNumberParameterOptions(_connection).Read();
			var discreteOptions = new DataHelperDiscreteParameterOptions(_connection).Read();
			var textOptions = new DataHelperTextParameterOptions(_connection).Read();
			var configurationParameters = new DataHelperConfigurationParameter(_connection).Read();

			return instances.Select(
					x =>
					{
						var configParameter = configurationParameters.First(p => p.ID == x.ConfigurationParameterValue.ConfigurationParameterReference);
						Models.NumberParameterOptions numberOption;
						if (x.ConfigurationParameterValue.NumberValueOptions != null)
						{
							numberOption = numberOptions.Find(o => o.ID == x.ConfigurationParameterValue.NumberValueOptions);
						}
						else if (configParameter.NumberOptions != null)
						{
							// Create duplicate of the pre-configured Configuration Parameter option
							numberOption = configParameter.NumberOptions;
							numberOption.ID = Guid.NewGuid();
						}
						else
						{
							numberOption = new Models.NumberParameterOptions
							{
								ID = Guid.NewGuid(),
							};
						}

						Models.DiscreteParameterOptions discreteOption;
						if (x.ConfigurationParameterValue.DiscreteValueOptions != null)
						{
							discreteOption = discreteOptions.Find(o => o.ID == x.ConfigurationParameterValue.DiscreteValueOptions);
						}
						else if (configParameter.DiscreteOptions != null)
						{
							// Create duplicate of the pre-configured Configuration Parameter option
							discreteOption = configParameter.DiscreteOptions;
							discreteOption.ID = Guid.NewGuid();
						}
						else
						{
							discreteOption = new Models.DiscreteParameterOptions
							{
								ID = Guid.NewGuid(),
							};
						}

						Models.TextParameterOptions textOption;
						if (x.ConfigurationParameterValue.TextValueOptions != null)
						{
							textOption = textOptions.Find(o => o.ID == x.ConfigurationParameterValue.TextValueOptions);
						}
						else if (configParameter.TextOptions != null)
						{
							// Create duplicate of the pre-configured Configuration Parameter option
							textOption = configParameter.TextOptions;
							textOption.ID = Guid.NewGuid();
						}
						else
						{
							textOption = new Models.TextParameterOptions
							{
								ID = Guid.NewGuid(),
							};
						}

						return new Models.ConfigurationParameterValue
						{
							ID = x.ID.Id,
							Label = x.ConfigurationParameterValue.Label,
							ConfigurationParameterId = x.ConfigurationParameterValue.ConfigurationParameterReference.Value,
							Type = x.ConfigurationParameterValue.Type ?? configParameter.Type,
							StringValue = x.ConfigurationParameterValue.StringValue,
							DoubleValue = x.ConfigurationParameterValue.DoubleValue,
							NumberOptions = numberOption,
							DiscreteOptions = discreteOption,
							TextOptions = textOption,
						};
					})
				.ToList();
		}

		public override Guid CreateOrUpdate(Models.ConfigurationParameterValue item)
		{
			var instance = new ConfigurationParameterValueInstance(New(item.ID));
			instance.ConfigurationParameterValue.Label = item.Label;
			instance.ConfigurationParameterValue.Type = item.Type;
			instance.ConfigurationParameterValue.StringValue = item.StringValue;
			instance.ConfigurationParameterValue.DoubleValue = item.DoubleValue;

			if (item.NumberOptions != null)
			{
				var numberOptionsHelper = new DataHelperNumberParameterOptions(_connection);
				instance.ConfigurationParameterValue.NumberValueOptions = numberOptionsHelper.CreateOrUpdate(item.NumberOptions);
			}

			if (item.DiscreteOptions != null)
			{
				var discreteOptionsHelper = new DataHelperDiscreteParameterOptions(_connection);
				instance.ConfigurationParameterValue.DiscreteValueOptions = discreteOptionsHelper.CreateOrUpdate(item.DiscreteOptions);
			}

			if (item.TextOptions != null)
			{
				var textOptionsHelper = new DataHelperTextParameterOptions(_connection);
				instance.ConfigurationParameterValue.TextValueOptions = textOptionsHelper.CreateOrUpdate(item.TextOptions);
			}

			if (item.ConfigurationParameterId == Guid.Empty)
			{
				item.ConfigurationParameterId = Guid.NewGuid();
			}

			var dataHelperConfigurationParameter = new DataHelperConfigurationParameter(_connection);
			var configurationParameters = dataHelperConfigurationParameter.Read();
			if (!configurationParameters.Exists(p => p.ID == item.ConfigurationParameterId))
			{
				// Create a default configuration parameter?
				dataHelperConfigurationParameter.CreateOrUpdate(new Models.ConfigurationParameter
				{
					ID = item.ConfigurationParameterId,
					Name = item.Label,
					Type = item.Type,
					NumberOptions = item.NumberOptions,
					DiscreteOptions = item.DiscreteOptions,
					TextOptions = item.TextOptions,
				});
			}

			instance.ConfigurationParameterValue.ConfigurationParameterReference = item.ConfigurationParameterId;

			return CreateOrUpdateInstance(instance);
		}
	}
}