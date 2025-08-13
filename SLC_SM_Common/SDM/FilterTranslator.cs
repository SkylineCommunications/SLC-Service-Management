namespace Skyline.DataMiner.SDM
{
	using System;
	using System.Linq;

	using DomHelpers.SlcConfigurations;
	using DomHelpers.SlcServicemanagement;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;

	public static class FilterTranslator
	{
		public static FilterElement<DomInstance> TranslateFullFilter<T>(FilterElement<T> filter) where T : class
		{
			if (filter is null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			FilterElement<DomInstance> translated;
			if (filter is ANDFilterElement<T> and)
			{
				translated = new ANDFilterElement<DomInstance>(and.subFilters.Select(TranslateFullFilter<T>).ToArray());
			}
			else if (filter is ORFilterElement<T> or)
			{
				translated = new ORFilterElement<DomInstance>(or.subFilters.Select(TranslateFullFilter).ToArray());
			}
			else if (filter is NOTFilterElement<T> not)
			{
				translated = new NOTFilterElement<DomInstance>(TranslateFullFilter(not));
			}
			else if (filter is TRUEFilterElement<T>)
			{
				translated = new TRUEFilterElement<DomInstance>();
			}
			else if (filter is FALSEFilterElement<T>)
			{
				translated = new FALSEFilterElement<DomInstance>();
			}
			else if (filter is ManagedFilterIdentifier managedFilter)
			{
				translated = TranslateFilter(managedFilter);
			}
			else
			{
				throw new NotSupportedException($"Unsupported filter: {filter}");
			}

			return translated;
		}

		private static FilterElement<DomInstance> TranslateFilter(ManagedFilterIdentifier managedFilter)
		{
			if (managedFilter is null)
			{
				throw new ArgumentNullException(nameof(managedFilter));
			}

			var fieldName = managedFilter.getFieldName().fieldName;
			var comparer = managedFilter.getComparer();
			var value = managedFilter.getValue();
			var translated = CreateFilter(fieldName, comparer, value);
			return translated;
		}

		private static FilterElement<DomInstance> CreateFilter(string fieldName, Comparer comparer, object value)
		{
			switch (fieldName)
			{
				/*SERVICE*/
				case "ServiceExposers.Guid":
					return FilterElementFactory.Create(DomInstanceExposers.Id, comparer, (Guid)value);
				case "ServiceExposers.ServiceName":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.ServiceName), comparer, (string)value);
				case "ServiceExposers.Description":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.Description), comparer, (string)value);
				case "ServiceExposers.ServiceStartTime":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.ServiceStartTime), comparer, (DateTime)value);
				case "ServiceExposers.ServiceEndTime":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.ServiceEndTime), comparer, (DateTime)value);
				case "ServiceExposers.Icon":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.Icon), comparer, (string)value);
				case "ServiceExposers.ServiceSpecifcation":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.ServiceSpecifcation), comparer, (Guid)value);
				case "ServiceExposers.ServiceProperties":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.ServiceProperties), comparer, (Guid)value);
				case "ServiceExposers.ServiceConfiguration":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.ServiceConfiguration), comparer, (Guid)value);
				case "ServiceExposers.RelatedOrganization":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.RelatedOrganization), comparer, (Guid)value);
				case "ServiceExposers.ServiceCategory":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.ServiceCategory), comparer, (value as Models.ServiceCategory)?.ID ?? Guid.Empty);
				case "ServiceExposers.ServiceConfigurationParameters":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.ServiceConfigurationParameters), comparer, (value as Models.ServiceConfigurationValue)?.ID ?? Guid.Empty);
				case "ServiceExposers.ServiceID":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.ServiceID), comparer, (string)value);
				case "ServiceItemsSection.Label":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItems.Label), comparer, (string)value);
				case "ServiceItemsSection.ServiceItemID":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItems.ServiceItemID), comparer, (long)value);
				case "ServiceItemsSection.ServiceItemType":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItems.ServiceItemType), comparer, (int)value);
				case "ServiceItemsSection.DefinitionReference":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItems.DefinitionReference), comparer, (string)value);
				case "ServiceItemsSection.ServiceItemConfiguration":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItems.ServiceItemConfiguration), comparer, (Guid)value);
				case "ServiceItemsSection.ServiceItemScript":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItems.ServiceItemScript), comparer, (string)value);
				case "ServiceItemsSection.ImplementationReference":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItems.ImplementationReference), comparer, (string)value);
				case "ServiceItemRelationshipSection.Type":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItemRelationship.Type), comparer, (string)value);
				case "ServiceItemRelationshipSection.ParentServiceItem":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItemRelationship.ParentServiceItem), comparer, (string)value);
				case "ServiceItemRelationshipSection.ChildServiceItem":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItemRelationship.ChildServiceItem), comparer, (string)value);
				case "ServiceItemRelationshipSection.ParentServiceItemInterfaceID":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItemRelationship.ParentServiceItemInterfaceID), comparer, (string)value);
				case "ServiceItemRelationshipSection.ChildServiceItemInterfaceID":
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItemRelationship.ChildServiceItemInterfaceID), comparer, (string)value);

				/*SERVICE CONFIGURATION VALUE*/
				case nameof(ServiceConfigurationValueExposers) + "." + nameof(ServiceConfigurationValueExposers.Guid):
					return FilterElementFactory.Create(DomInstanceExposers.Id, comparer, (Guid)value);
				case nameof(ServiceConfigurationValueExposers) + "." + nameof(ServiceConfigurationValueExposers.ConfigurationParameterID):
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceConfigurationValue.ConfigurationParameterValue), comparer, (Guid)value);

				/*CONFIGURATION PARAM*/
				case nameof(ConfigurationParameterExposers) + "." + nameof(ConfigurationParameterExposers.Guid):
					return FilterElementFactory.Create(DomInstanceExposers.Id, comparer, (Guid)value);
				case nameof(ConfigurationParameterExposers) + "." + nameof(ConfigurationParameterExposers.Name):
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcConfigurationsIds.Sections.ConfigurationParameterInfo.ParameterName), comparer, (string)value);

				/*CONFIGURATION VALUE*/
				case nameof(ConfigurationParameterValueExposers) + "." + nameof(ConfigurationParameterValueExposers.Guid):
					return FilterElementFactory.Create(DomInstanceExposers.Id, comparer, (Guid)value);
				case nameof(ConfigurationParameterValueExposers) + "." + nameof(ConfigurationParameterValueExposers.Label):
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcConfigurationsIds.Sections.ConfigurationParameterValue.Label), comparer, (string)value);
				case nameof(ConfigurationParameterValueExposers) + "." + nameof(ConfigurationParameterValueExposers.StringValue):
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcConfigurationsIds.Sections.ConfigurationParameterValue.StringValue), comparer, (string)value);
				case nameof(ConfigurationParameterValueExposers) + "." + nameof(ConfigurationParameterValueExposers.DoubleValue):
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcConfigurationsIds.Sections.ConfigurationParameterValue.DoubleValue), comparer, (double?)value);
				case nameof(ConfigurationParameterValueExposers) + "." + nameof(ConfigurationParameterValueExposers.Type):
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcConfigurationsIds.Sections.ConfigurationParameterValue.Type), comparer, (SlcConfigurationsIds.Enums.Type)value);
				case nameof(ConfigurationParameterValueExposers) + "." + nameof(ConfigurationParameterValueExposers.ConfigurationParameterID):
					return FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcConfigurationsIds.Sections.ConfigurationParameterValue.ConfigurationParameterReference), comparer, (Guid)value);
				default:
					throw new NotSupportedException(fieldName);
			}
		}
	}
}