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
	using Skyline.DataMiner.SDM;

	/// <inheritdoc />
	public class DataHelperServiceConfigurationValue : DataHelper<Models.ServiceConfigurationValue>
	{
		/// <inheritdoc />
		public DataHelperServiceConfigurationValue(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.ServiceConfigurationValue)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ServiceConfigurationValue item)
		{
			var instance = new ServiceConfigurationValueInstance(New(item.ID));
			instance.ServiceConfigurationValue.MandatoryAtServiceLevel = item.Mandatory;

			if (item.ConfigurationParameter.ID == Guid.Empty)
			{
				item.ConfigurationParameter.ID = Guid.NewGuid();
			}

			var dataHelperConfigurationParameters = new DataHelperConfigurationParameterValue(_connection);
			dataHelperConfigurationParameters.CreateOrUpdate(item.ConfigurationParameter);
			instance.ServiceConfigurationValue.ConfigurationParameterValue = item.ConfigurationParameter.ID;

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public List<Models.ServiceConfigurationValue> Read(FilterElement<Models.ServiceConfigurationValue> filter)
		{
			if (filter is null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			var domFilter = FilterTranslator.TranslateFullFilter(filter);
			return Read(_domHelper.DomInstances.Read(domFilter));
		}

		/// <inheritdoc />
		public override List<Models.ServiceConfigurationValue> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id));
			return Read(instances);
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.ServiceConfigurationValue item)
		{
			bool ok = TryDelete(item.ConfigurationParameter.ID);
			return ok && TryDelete(item.ID);
		}

		private List<Models.ServiceConfigurationValue> Read(List<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new ServiceConfigurationValueInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.ServiceConfigurationValue>();
			}

			var dataHelperConfigurationParameters = new DataHelperConfigurationParameterValue(_connection);
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
	}
}