namespace SLC_SM_Common.API.ServiceManagementApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class DataHelperServiceOrderItemConfigurationValue : DataHelper<Models.ServiceOrderItemConfigurationValue>
	{
		public DataHelperServiceOrderItemConfigurationValue(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.ServiceOrderItemConfigurationValue)
		{
		}

		public override List<Models.ServiceOrderItemConfigurationValue> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new ServiceOrderItemConfigurationValueInstance(x))
				.ToList();

			var dataHelperConfigurationParameters = new ConfigurationsApi.DataHelperConfigurationParameterValue(_connection);
			var configurationParameters = dataHelperConfigurationParameters.Read();

			return instances.Select(
					x => new Models.ServiceOrderItemConfigurationValue
					{
						ID = x.ID.Id,
						Mandatory = x.ServiceOrderItemConfigurationValue.MandatoryAtServiceOrderLevel ?? false,
						ConfigurationParameter = configurationParameters.Find(p => p.ID == x.ServiceOrderItemConfigurationValue.ConfigurationParameterValue),
					})
				.ToList();
		}

		public override Guid CreateOrUpdate(Models.ServiceOrderItemConfigurationValue item)
		{
			var instance = new ServiceOrderItemConfigurationValueInstance(New(item.ID));
			instance.ServiceOrderItemConfigurationValue.MandatoryAtServiceOrderLevel = item.Mandatory;

			if (item.ConfigurationParameter.ID == Guid.Empty)
			{
				item.ConfigurationParameter.ID = Guid.NewGuid();
			}

			var dataHelperConfigurationParameters = new ConfigurationsApi.DataHelperConfigurationParameterValue(_connection);
			dataHelperConfigurationParameters.CreateOrUpdate(item.ConfigurationParameter);
			instance.ServiceOrderItemConfigurationValue.ConfigurationParameterValue = item.ConfigurationParameter.ID;

			return CreateOrUpdateInstance(instance);
		}
	}
}