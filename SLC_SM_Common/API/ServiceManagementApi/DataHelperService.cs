namespace SLC_SM_Common.API.ServiceManagementApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class DataHelperService : DataHelper<Models.Service>
	{
		public DataHelperService(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.Services)
		{
		}

		public override List<Models.Service> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new ServicesInstance(x))
				.ToList();

			var dataHelperServicePropertyValues = new DataHelperServicePropertyValues(_connection);
			var serviceProperties = dataHelperServicePropertyValues.Read();
			var dataHelperServiceConfigurations = new DataHelperServiceConfigurationValue(_connection);
			var serviceConfigurations = dataHelperServiceConfigurations.Read();
			var dataHelperServiceCategory = new DataHelperServiceCategory(_connection);
			var serviceCategories = dataHelperServiceCategory.Read();

			return instances.Select(
					x => new Models.Service
					{
						ID = x.ID.Id,
						Name = x.ServiceInfo.ServiceName,
						Description = x.ServiceInfo.Description,
						StartTime = x.ServiceInfo.ServiceStartTime,
						EndTime = x.ServiceInfo.ServiceEndTime,
						Icon = x.ServiceInfo.Icon,
						Category = serviceCategories.Find(c => c.ID == x.ServiceInfo.ServiceCategory),
						ServiceSpecificationId = x.ServiceInfo.ServiceSpecifcation,
						Properties = serviceProperties.Find(p => p.ID == x.ServiceInfo.ServiceProperties) ?? new Models.ServicePropertyValues { Values = new List<Models.ServicePropertyValue>() },
						Configurations = serviceConfigurations.Where(p => x.ServiceInfo.ServiceConfigurationParameters.Contains(p.ID)).ToList(),
						ServiceItems = x.ServiceItems.Select(s => new Models.ServiceItem
						{
							ID = s.ServiceItemID ?? 1,
							Label = s.Label ?? String.Empty,
							Type = s.ServiceItemType ?? SlcServicemanagementIds.Enums.ServiceitemtypesEnum.SRMBooking,
							Script = s.ServiceItemScript,
							DefinitionReference = s.DefinitionReference,
							ImplementationReference = s.ImplementationReference,
						}).ToList(),
						ServiceItemsRelationships = x.ServiceItemRelationship.Select(r => new Models.ServiceItemRelationShip
						{
							ParentServiceItem = r.ParentServiceItem,
							ParentServiceItemInterfaceId = r.ParentServiceItemInterfaceID,
							ChildServiceItem = r.ChildServiceItem,
							ChildServiceItemInterfaceId = r.ChildServiceItemInterfaceID,
							Type = r.Type,
						}).ToList(),
					})
				.ToList();
		}

		public override Guid CreateOrUpdate(Models.Service item)
		{
			DomInstance domInstance = New(item.ID);
			var existingStatusId = _domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(domInstance.ID)).FirstOrDefault()?.StatusId;
			if (existingStatusId != null)
			{
				domInstance.StatusId = existingStatusId;
			}

			var instance = new ServicesInstance(domInstance);
			instance.ServiceInfo.ServiceName = item.Name;
			instance.ServiceInfo.Description = item.Description;
			instance.ServiceInfo.ServiceStartTime = item.StartTime;
			instance.ServiceInfo.ServiceEndTime = item.EndTime;
			instance.ServiceInfo.ServiceProperties = item.Properties?.ID;
			instance.ServiceInfo.ServiceCategory = item.Category?.ID;
			instance.ServiceInfo.ServiceSpecifcation = item.ServiceSpecificationId;
			instance.ServiceInfo.Icon = item.Icon;

			if (item.Properties != null)
			{
				var dataHelperProperties = new DataHelperServicePropertyValues(_connection);
				dataHelperProperties.CreateOrUpdate(item.Properties);
			}

			if (item.Category != null)
			{
				var dataHelperServiceCategory = new DataHelperServiceCategory(_connection);
				dataHelperServiceCategory.CreateOrUpdate(item.Category);
			}

			instance.ServiceItemRelationship.Clear();
			if (item.ServiceItemsRelationships != null)
			{
				foreach (var relationship in item.ServiceItemsRelationships)
				{
					instance.ServiceItemRelationship.Add(
						new ServiceItemRelationshipSection
						{
							ParentServiceItem = relationship.ParentServiceItem,
							ParentServiceItemInterfaceID = relationship.ParentServiceItemInterfaceId,
							ChildServiceItem = relationship.ChildServiceItem,
							ChildServiceItemInterfaceID = relationship.ChildServiceItemInterfaceId,
							Type = relationship.Type,
						});
				}
			}

			if (!instance.ServiceItemRelationship.Any())
			{
				instance.ServiceItemRelationship.Add(new ServiceItemRelationshipSection());
			}

			var dataHelperConfigurations = new DataHelperServiceConfigurationValue(_connection);
			instance.ServiceInfo.ServiceConfigurationParameters.Clear();
			if (item.Configurations != null)
			{
				foreach (var config in item.Configurations)
				{
					instance.ServiceInfo.ServiceConfigurationParameters.Add(dataHelperConfigurations.CreateOrUpdate(config));
				}
			}

			instance.ServiceItems.Clear();
			if (item.ServiceItems != null)
			{
				foreach (var si in item.ServiceItems)
				{
					instance.ServiceItems.Add(
						new ServiceItemsSection
						{
							ServiceItemID = si.ID,
							Label = si.Label,
							ServiceItemScript = si.Script,
							ServiceItemType = si.Type,
							DefinitionReference = si.DefinitionReference,
							ImplementationReference = si.ImplementationReference,
						});
				}
			}

			if (!instance.ServiceItems.Any())
			{
				instance.ServiceItems.Add(new ServiceItemsSection());
			}

			return CreateOrUpdateInstance(instance);
		}

		public override bool TryDelete(Models.Service item)
		{
			bool b = TryDelete(item.Properties.ID);
			foreach (var config in item.Configurations)
			{
				b &= TryDelete(config.ID);
			}

			return b && TryDelete(item.ID);
		}
	}
}