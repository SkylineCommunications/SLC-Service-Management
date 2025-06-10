namespace SLC_SM_Common.API.ServiceManagementApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class DataHelperServiceSpecification : DataHelper<Models.ServiceSpecification>
	{
		public DataHelperServiceSpecification(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.ServiceSpecifications)
		{
		}

		public override List<Models.ServiceSpecification> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new ServiceSpecificationsInstance(x))
				.ToList();

			var dataHelperServicePropertyValues = new DataHelperServicePropertyValues(_connection);
			var serviceProperties = dataHelperServicePropertyValues.Read();
			var dataHelperServiceConfigurations = new DataHelperServiceSpecificationConfigurationValue(_connection);
			var serviceConfigurations = dataHelperServiceConfigurations.Read();

			return instances.Select(
					x => new Models.ServiceSpecification
					{
						ID = x.ID.Id,
						Name = x.ServiceSpecificationInfo.SpecificationName,
						Properties = serviceProperties.Find(p => p.ID == x.ServiceSpecificationInfo.ServiceProperties) ?? new Models.ServicePropertyValues { Values = new List<Models.ServicePropertyValue>() },
						Configurations = serviceConfigurations.Where(p => x.ServiceSpecificationInfo.ServiceSpecificationConfigurationParameters.Contains(p.ID)).ToList(),
						ServiceItems = x.ServiceItems.Select(s => new Models.ServiceItem
						{
							ID = s.ServiceItemID ?? 1,
							Label = s.Label ?? String.Empty,
							Type = s.ServiceItemType ?? SlcServicemanagementIds.Enums.ServiceitemtypesEnum.SRMBooking,
							Script = s.ServiceItemScript,
						}).ToList(),
					})
				.ToList();
		}

		public override Guid CreateOrUpdate(Models.ServiceSpecification item)
		{
			var instance = new ServiceSpecificationsInstance(New(item.ID));
			instance.ServiceSpecificationInfo.SpecificationName = item.Name;
			instance.ServiceSpecificationInfo.Description = item.Name;
			instance.ServiceSpecificationInfo.ServiceProperties = item.Properties?.ID;

			if (item.Properties != null)
			{
				var dataHelperProperties = new DataHelperServicePropertyValues(_connection);
				dataHelperProperties.CreateOrUpdate(item.Properties);
			}

			instance.ServiceItemRelationship.Add(new ServiceItemRelationshipSection());

			var dataHelperConfigurations = new DataHelperServiceSpecificationConfigurationValue(_connection);
			foreach (var config in item.Configurations)
			{
				instance.ServiceSpecificationInfo.ServiceSpecificationConfigurationParameters.Add(config.ID);
				dataHelperConfigurations.CreateOrUpdate(config);
			}

			foreach (var si in item.ServiceItems)
			{
				instance.ServiceItems.Add(new ServiceItemsSection
				{
					ServiceItemID = si.ID,
					Label = si.Label,
					ServiceItemScript = si.Script,
					ServiceItemType = si.Type,
				});
			}

			return CreateOrUpdateInstance(instance);
		}
	}
}