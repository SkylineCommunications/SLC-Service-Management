namespace DomHelpers.SlcServicemanagement
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	public interface IServiceInstanceBase
	{
		IList<ServiceItemsSection> GetServiceItems();

		IList<ServiceItemRelationshipSection> GetServiceItemRelationships();

		DomInstanceId GetId();

		string GetName();

		void Save(DomHelper domHelper);
	}

	public partial class ServiceSpecificationsInstance : IServiceInstanceBase
	{
		public IList<ServiceItemsSection> GetServiceItems() => ServiceItems;

		public IList<ServiceItemRelationshipSection> GetServiceItemRelationships() => ServiceItemRelationship;

		public DomInstanceId GetId() => ID;

		public string GetName() => Name;
	}

	public partial class ServicesInstance : IServiceInstanceBase
	{
		public IList<ServiceItemsSection> GetServiceItems() => ServiceItems;

		public IList<ServiceItemRelationshipSection> GetServiceItemRelationships() => ServiceItemRelationship;

		public DomInstanceId GetId() => ID;

		public string GetName() => Name;
	}

	public class ServiceInstancesExtentions
	{
		public static IServiceInstanceBase GetTypedInstance(DomInstance instance)
		{
			if (instance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceSpecifications.Id)
				return new ServiceSpecificationsInstance(instance);

			if (instance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.Services.Id)
				return new ServicesInstance(instance);

			throw new InvalidOperationException($"Unsupported Dom Definition: {instance.DomDefinitionId.Id}");
		}
	}
}
