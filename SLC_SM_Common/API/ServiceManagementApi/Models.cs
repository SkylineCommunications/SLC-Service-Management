namespace SLC_SM_Common.API
{
	using System;
	using System.Collections.Generic;

	using DomHelpers.SlcServicemanagement;

	namespace ServiceManagementApi
	{
		public static class Models
		{
			public class ServiceSpecification
			{
				public Guid ID { get; set; }

				public string Name { get; set; }

				public string Description { get; set; }

				public string Icon { get; set; }

				public ServicePropertyValues Properties { get; set; }

				public List<ServiceSpecificationConfigurationValue> Configurations { get; set; }

				public List<ServiceItem> ServiceItems { get; set; }

				public List<ServiceItemRelationShip> ServiceItemsRelationships { get; set; }
			}

			public class ServiceItem
			{
				public long ID { get; set; }

				public string Label { get; set; }

				public SlcServicemanagementIds.Enums.ServiceitemtypesEnum Type { get; set; }

				public string Script { get; set; }

				public string DefinitionReference { get; set; }

				public string ImplementationReference { get; set; }
			}

			public class ServiceOrder
			{
				public Guid ID { get; set; }

				public string StatusId { get; set; }

				public string Name { get; set; }

				public string ExternalID { get; set; }

				public SlcServicemanagementIds.Enums.ServiceorderpriorityEnum? Priority { get; set; }

				public string Description { get; set; }

				public Guid? OrganizationId { get; set; }

				public IList<Guid> ContactIds { get; set; }

				public List<ServiceOrderItems> OrderItems { get; set; }
			}

			public class ServiceOrderItems
			{
				public long? Priority { get; set; }

				public ServiceOrderItem ServiceOrderItem { get; set; }
			}

			public class ServiceOrderItem
			{
				public Guid ID { get; set; }

				public string StatusId { get; set; }

				public string Name { get; set; }

				public string Action { get; set; }

				public DateTime? StartTime { get; set; }

				public DateTime? EndTime { get; set; }

				public bool? IndefiniteRuntime { get; set; }

				public Guid? SpecificationId { get; set; }

				public Guid? ServiceCategoryId { get; set; }

				public Guid? ServiceId { get; set; }

				public ServicePropertyValues Properties { get; set; }

				public List<ServiceOrderItemConfigurationValue> Configurations { get; set; }
			}

			public class ServicePropertyValues
			{
				public Guid ID { get; set; }

				public List<ServicePropertyValue> Values { get; set; }
			}

			public class ServicePropertyValue
			{
				public string Value { get; set; }

				public Guid ServicePropertyId { get; set; }
			}

			public class ServiceProperty
			{
				public Guid ID { get; set; }

				public string Name { get; set; }

				public SlcServicemanagementIds.Enums.TypeEnum Type { get; set; }

				public List<string> DiscreteValues { get; set; }
			}

			public class ServiceConfigurationValue
			{
				public Guid ID { get; set; }

				public ConfigurationsApi.Models.ConfigurationParameterValue ConfigurationParameter { get; set; }

				public bool Mandatory { get; set; }
			}

			public class ServiceOrderItemConfigurationValue
			{
				public Guid ID { get; set; }

				public ConfigurationsApi.Models.ConfigurationParameterValue ConfigurationParameter { get; set; }

				public bool Mandatory { get; set; }
			}

			public class ServiceSpecificationConfigurationValue
			{
				public Guid ID { get; set; }

				public ConfigurationsApi.Models.ConfigurationParameterValue ConfigurationParameter { get; set; }

				public bool MandatoryAtService { get; set; }

				public bool MandatoryAtServiceOrder { get; set; }

				public bool ExposeAtServiceOrder { get; set; }
			}

			public class ServiceCategory
			{
				public Guid ID { get; set; }

				public string Name { get; set; }

				public string Type { get; set; }
			}

			public class Service
			{
				public Guid ID { get; set; }

				public string Name { get; set; }

				public string Description { get; set; }

				public string Icon { get; set; }

				public DateTime? StartTime { get; set; }

				public DateTime? EndTime { get; set; }

				public ServiceCategory Category { get; set; }

				public Guid? OrganizationId { get; set; }

				public Guid? ServiceSpecificationId { get; set; }

				public ServicePropertyValues Properties { get; set; }

				public List<ServiceConfigurationValue> Configurations { get; set; }

				public List<ServiceItem> ServiceItems { get; set; }

				public List<ServiceItemRelationShip> ServiceItemsRelationships { get; set; }
			}

			public class ServiceItemRelationShip
			{
				public string Type { get; set; }

				public string ParentServiceItem { get; set; }

				public string ParentServiceItemInterfaceId { get; set; }

				public string ChildServiceItem { get; set; }

				public string ChildServiceItemInterfaceId { get; set; }
			}
		}
	}
}