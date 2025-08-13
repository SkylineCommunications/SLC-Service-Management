namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations;

	/// <inheritdoc />
	public class DataHelperServiceOrderItemConfigurationValue : DataHelper<Models.ServiceOrderItemConfigurationValue>
	{
		/// <inheritdoc />
		public DataHelperServiceOrderItemConfigurationValue(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.ServiceOrderItemConfigurationValue)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ServiceOrderItemConfigurationValue item)
		{
			var instance = new ServiceOrderItemConfigurationValueInstance(New(item.ID));
			instance.ServiceOrderItemConfigurationValue.MandatoryAtServiceOrderLevel = item.Mandatory;

			if (item.ConfigurationParameter.ID == Guid.Empty)
			{
				item.ConfigurationParameter.ID = Guid.NewGuid();
			}

			var dataHelperConfigurationParameters = new DataHelperConfigurationParameterValue(_connection);
			instance.ServiceOrderItemConfigurationValue.ConfigurationParameterValue = dataHelperConfigurationParameters.CreateOrUpdate(item.ConfigurationParameter);

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override List<Models.ServiceOrderItemConfigurationValue> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new ServiceOrderItemConfigurationValueInstance(x))
				.ToList();

			var dataHelperConfigurationParameters = new DataHelperConfigurationParameterValue(_connection);
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

		/// <inheritdoc />
		public override bool TryDelete(Models.ServiceOrderItemConfigurationValue item)
		{
			bool ok = TryDelete(item.ConfigurationParameter.ID);
			return ok && TryDelete(item.ID);
		}
	}
}