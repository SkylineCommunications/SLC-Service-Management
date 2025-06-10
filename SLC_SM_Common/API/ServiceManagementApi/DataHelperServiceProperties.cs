namespace SLC_SM_Common.API.ServiceManagementApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class DataHelperServiceProperties : DataHelper<Models.ServiceProperty>
	{
		public DataHelperServiceProperties(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.ServiceProperties)
		{
		}

		public override List<Models.ServiceProperty> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new ServicePropertiesInstance(x))
				.ToList();

			return instances.Select(
					x => new Models.ServiceProperty
					{
						ID = x.ID.Id,
						Name = x.ServicePropertyInfo.Name,
						Type = x.ServicePropertyInfo.Type ?? SlcServicemanagementIds.Enums.TypeEnum.String,
						DiscreteValues = x.DiscreteServicePropertyValueOptions.Select(d => d.DiscreteValue).ToList(),
					})
				.ToList();
		}

		public override Guid CreateOrUpdate(Models.ServiceProperty item)
		{
			var instance = new ServicePropertiesInstance(New(item.ID));
			instance.ServicePropertyInfo.Name = item.Name;
			instance.ServicePropertyInfo.Type = item.Type;

			foreach (string discreteValue in item.DiscreteValues)
			{
				instance.DiscreteServicePropertyValueOptions.Add(new DiscreteServicePropertyValueOptionsSection
				{
					DiscreteValue = discreteValue,
				});
			}

			return CreateOrUpdateInstance(instance);
		}
	}
}