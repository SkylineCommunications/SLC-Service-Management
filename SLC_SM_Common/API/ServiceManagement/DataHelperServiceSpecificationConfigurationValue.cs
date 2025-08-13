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
	public class DataHelperServiceSpecificationConfigurationValue : DataHelper<Models.ServiceSpecificationConfigurationValue>
	{
		/// <inheritdoc />
		public DataHelperServiceSpecificationConfigurationValue(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.ServiceSpecificationConfigurationValue)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ServiceSpecificationConfigurationValue item)
		{
			var instance = new ServiceSpecificationConfigurationValueInstance(New(item.ID));
			instance.ServiceSpecificationConfigurationValue.MandatoryAtServiceOrderLevel = item.MandatoryAtServiceOrder;
			instance.ServiceSpecificationConfigurationValue.MandatoryAtServiceLevel = item.MandatoryAtService;
			instance.ServiceSpecificationConfigurationValue.ExposeAtServiceOrderLevel = item.ExposeAtServiceOrder;

			if (item.ConfigurationParameter.ID == Guid.Empty)
			{
				item.ConfigurationParameter.ID = Guid.NewGuid();
			}

			var dataHelperConfigurationParameters = new DataHelperConfigurationParameterValue(_connection);
			instance.ServiceSpecificationConfigurationValue.ConfigurationParameterValue = dataHelperConfigurationParameters.CreateOrUpdate(item.ConfigurationParameter);

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override List<Models.ServiceSpecificationConfigurationValue> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new ServiceSpecificationConfigurationValueInstance(x))
				.ToList();

			var dataHelperConfigurationParameters = new DataHelperConfigurationParameterValue(_connection);
			var configurationParameters = dataHelperConfigurationParameters.Read();

			return instances.Select(
					x => new Models.ServiceSpecificationConfigurationValue
					{
						ID = x.ID.Id,
						MandatoryAtService = x.ServiceSpecificationConfigurationValue.MandatoryAtServiceLevel ?? false,
						MandatoryAtServiceOrder = x.ServiceSpecificationConfigurationValue.MandatoryAtServiceOrderLevel ?? false,
						ExposeAtServiceOrder = x.ServiceSpecificationConfigurationValue.ExposeAtServiceOrderLevel ?? false,
						ConfigurationParameter = configurationParameters.Find(p => p.ID == x.ServiceSpecificationConfigurationValue.ConfigurationParameterValue),
					})
				.ToList();
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.ServiceSpecificationConfigurationValue item)
		{
			var helper = new DataHelperConfigurationParameterValue(_connection);
			bool b = item.ConfigurationParameter == null || helper.TryDelete(item.ConfigurationParameter.ID);

			return b && TryDelete(item.ID);
		}
	}
}