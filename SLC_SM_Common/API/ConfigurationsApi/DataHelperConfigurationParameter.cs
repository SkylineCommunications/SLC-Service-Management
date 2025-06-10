namespace SLC_SM_Common.API.ConfigurationsApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcConfigurations;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class DataHelperConfigurationParameter : DataHelper<Models.ConfigurationParameter>
	{
		public DataHelperConfigurationParameter(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.ConfigurationParameters)
		{
		}

		public override List<Models.ConfigurationParameter> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new ConfigurationParametersInstance(x))
				.ToList();

			var numberOptions = new DataHelperNumberParameterOptions(_connection).Read();
			var discreteOptions = new DataHelperDiscreteParameterOptions(_connection).Read();
			var textOptions = new DataHelperTextParameterOptions(_connection).Read();

			return instances.Select(
					x => new Models.ConfigurationParameter
					{
						ID = x.ID.Id,
						Name = x.ConfigurationParameterInfo.ParameterName,
						Type = x.ConfigurationParameterInfo.Type ?? SlcConfigurationsIds.Enums.ParameterType.Text,
						NumberOptions = numberOptions.Find(o => o.ID == x.ConfigurationParameterInfo.NumberOptions),
						DiscreteOptions = discreteOptions.Find(o => o.ID == x.ConfigurationParameterInfo.DiscreteOptions),
						TextOptions = textOptions.Find(o => o.ID == x.ConfigurationParameterInfo.TextOptions),
					})
				.ToList();
		}

		public override Guid CreateOrUpdate(Models.ConfigurationParameter item)
		{
			var instance = new ConfigurationParametersInstance(New(item.ID));
			instance.ConfigurationParameterInfo.ParameterName = item.Name;
			instance.ConfigurationParameterInfo.Type = item.Type;

			if (item.NumberOptions != null)
			{
				var numberOptionsHelper = new DataHelperNumberParameterOptions(_connection);
				instance.ConfigurationParameterInfo.NumberOptions = numberOptionsHelper.CreateOrUpdate(item.NumberOptions);
			}

			if (item.DiscreteOptions != null)
			{
				var discreteOptionsHelper = new DataHelperDiscreteParameterOptions(_connection);
				instance.ConfigurationParameterInfo.DiscreteOptions = discreteOptionsHelper.CreateOrUpdate(item.DiscreteOptions);
			}

			if (item.TextOptions != null)
			{
				var textOptionsHelper = new DataHelperTextParameterOptions(_connection);
				instance.ConfigurationParameterInfo.TextOptions = textOptionsHelper.CreateOrUpdate(item.TextOptions);
			}

			return CreateOrUpdateInstance(instance);
		}
	}
}