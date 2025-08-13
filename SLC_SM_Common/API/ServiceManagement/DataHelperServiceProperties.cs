namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement
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

		public override Guid CreateOrUpdate(Models.ServiceProperty item)
		{
			var instance = new ServicePropertiesInstance(New(item.ID));
			instance.ServicePropertyInfo.Name = item.Name;
			instance.ServicePropertyInfo.Type = item.Type;

			if (item.DiscreteValues != null)
			{
				foreach (string discreteValue in item.DiscreteValues)
				{
					instance.DiscreteServicePropertyValueOptions.Add(
						new DiscreteServicePropertyValueOptionsSection
						{
							DiscreteValue = discreteValue,
						});
				}
			}

			if (!instance.DiscreteServicePropertyValueOptions.Any())
			{
				instance.DiscreteServicePropertyValueOptions.Add(new DiscreteServicePropertyValueOptionsSection());
			}

			return CreateOrUpdateInstance(instance);
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

		public override bool TryDelete(Models.ServiceProperty item)
		{
			return TryDelete(item.ID);
		}
	}
}