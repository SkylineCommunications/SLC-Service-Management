namespace SLC_SM_Common.API.ServiceManagementApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class DataHelperServiceConfigurationValue : DataHelper<Models.ServiceConfigurationValue>
	{
		public DataHelperServiceConfigurationValue(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.ServiceConfigurationValue)
		{
		}

		public override List<Models.ServiceConfigurationValue> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new ServiceConfigurationValueInstance(x))
				.ToList();

			var dataHelperConfigurationParameters = new ConfigurationsApi.DataHelperConfigurationParameterValue(_connection);
			var configurationParameters = dataHelperConfigurationParameters.Read();

			return instances.Select(
					x => new Models.ServiceConfigurationValue
					{
						ID = x.ID.Id,
						Mandatory = x.ServiceConfigurationValue.MandatoryAtServiceLevel ?? false,
						ConfigurationParameter = configurationParameters.Find(p => p.ID == x.ServiceConfigurationValue.ConfigurationParameterValue),
					})
				.ToList();
		}

		public override Guid CreateOrUpdate(Models.ServiceConfigurationValue item)
		{
			var instance = new ServiceConfigurationValueInstance(New(item.ID));
			instance.ServiceConfigurationValue.MandatoryAtServiceLevel = item.Mandatory;

			if (item.ConfigurationParameter.ID == Guid.Empty)
			{
				item.ConfigurationParameter.ID = Guid.NewGuid();
			}

			var dataHelperConfigurationParameters = new ConfigurationsApi.DataHelperConfigurationParameterValue(_connection);
			dataHelperConfigurationParameters.CreateOrUpdate(item.ConfigurationParameter);
			instance.ServiceConfigurationValue.ConfigurationParameterValue = item.ConfigurationParameter.ID;

			return CreateOrUpdateInstance(instance);
		}

		public override bool TryDelete(Models.ServiceConfigurationValue item)
		{
			bool ok = TryDelete(item.ConfigurationParameter.ID);
			return ok && TryDelete(item.ID);
		}
	}
}