namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <inheritdoc />
	public class DataHelperServiceSpecification : DataHelper<Models.ServiceSpecification>
	{
		/// <inheritdoc />
		public DataHelperServiceSpecification(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.ServiceSpecifications)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ServiceSpecification item)
		{
			var instance = new ServiceSpecificationsInstance(New(item.ID));
			instance.ServiceSpecificationInfo.SpecificationName = item.Name;
			instance.ServiceSpecificationInfo.Description = item.Description;
			instance.ServiceSpecificationInfo.Icon = item.Icon;
			instance.ServiceSpecificationInfo.ServiceProperties = item.Properties?.ID;

			if (item.Properties != null)
			{
				var dataHelperProperties = new DataHelperServicePropertyValues(_connection);
				dataHelperProperties.CreateOrUpdate(item.Properties);
			}

			if (item.ServiceItemsRelationships != null)
			{
				foreach (var relationship in item.ServiceItemsRelationships.Where(r => r != null))
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

			var dataHelperConfigurations = new DataHelperServiceSpecificationConfigurationValue(_connection);
			if (item.Configurations != null)
			{
				foreach (var config in item.Configurations.Where(c => c?.ConfigurationParameter != null))
				{
					instance.ServiceSpecificationInfo.ServiceSpecificationConfigurationParameters.Add(dataHelperConfigurations.CreateOrUpdate(config));
				}
			}

			if (item.ServiceItems != null)
			{
				foreach (var si in item.ServiceItems.Where(s => s != null))
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

		/// <inheritdoc />
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
						Description = x.ServiceSpecificationInfo.Description,
						Icon = x.ServiceSpecificationInfo.Icon,
						Properties = serviceProperties.Find(p => p.ID == x.ServiceSpecificationInfo.ServiceProperties)
						             ?? new Models.ServicePropertyValues { Values = new List<Models.ServicePropertyValue>() },
						Configurations = serviceConfigurations.Where(p => x.ServiceSpecificationInfo.ServiceSpecificationConfigurationParameters.Contains(p.ID)).ToList(),
						ServiceItems = x.ServiceItems.Select(
								s => new Models.ServiceItem
								{
									ID = s.ServiceItemID ?? 1,
									Label = s.Label ?? String.Empty,
									Type = s.ServiceItemType ?? SlcServicemanagementIds.Enums.ServiceitemtypesEnum.SRMBooking,
									Script = s.ServiceItemScript ?? String.Empty,
									DefinitionReference = s.DefinitionReference ?? String.Empty,
									ImplementationReference = s.ImplementationReference ?? String.Empty,
								})
							.Where(s => !String.IsNullOrEmpty(s.Label))
							.ToList(),
						ServiceItemsRelationships = x.ServiceItemRelationship.Select(
								r => new Models.ServiceItemRelationShip
								{
									ParentServiceItem = r.ParentServiceItem,
									ParentServiceItemInterfaceId = r.ParentServiceItemInterfaceID,
									ChildServiceItem = r.ChildServiceItem,
									ChildServiceItemInterfaceId = r.ChildServiceItemInterfaceID,
									Type = r.Type,
								})
							.ToList(),
					})
				.ToList();
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.ServiceSpecification item)
		{
			bool b = true;

			if (item.Properties != null)
			{
				b &= new DataHelperServicePropertyValues(_connection).TryDelete(item.Properties);
			}

			if (item.Configurations != null)
			{
				var helperConfigs = new DataHelperServiceSpecificationConfigurationValue(_connection);
				foreach (var config in item.Configurations)
				{
					b &= helperConfigs.TryDelete(config.ID);
				}
			}

			return b && TryDelete(item.ID);
		}
	}
}