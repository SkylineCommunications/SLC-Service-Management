namespace SLC_SM_IAS_ManageRelationships.Model
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using DomHelpers.SlcWorkflow;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using SLC_SM_IAS_ManageRelationships.Controller;

	internal class ManageConnectionsModel
	{
		private readonly IEngine _engine;
		private readonly DomHelper _smDomHelper;
		private readonly DomHelper _wfDomHelper;

		public ManageConnectionsModel(IEngine engine)
		{
			_engine = engine;
			_smDomHelper = new DomHelper(_engine.SendSLNetMessages, SlcServicemanagementIds.ModuleId);
			_wfDomHelper = new DomHelper(_engine.SendSLNetMessages, SlcWorkflowIds.ModuleId);
		}

		public WorkflowsInstance GetWorkflowbyId(Guid workflowId)
		{
			var domInstance = _wfDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(workflowId)).FirstOrDefault();
			if (domInstance == null)
				throw new InvalidOperationException($"Could not find workflow with id {workflowId}");

			return new WorkflowsInstance(domInstance);
		}

		public WorkflowsInstance GetWorkflowbyName(string workflowName)
		{
			var domInstance = _wfDomHelper.DomInstances.Read(DomInstanceExposers.Name.Equal(workflowName)).FirstOrDefault();
			if (domInstance == null)
				throw new InvalidOperationException($"Could not find workflow with id {workflowName}");

			return new WorkflowsInstance(domInstance);
		}

		/// <summary>
		/// Turns a list of Service Items (A,B,C,D) to a list of pairs (A,B), (B,C), (C,D).
		/// This allows building the relationships between Service Items in the same order the user selected them.
		/// </summary>
		/// <param name="source">A list of Service Items to be connected in sequence.</param>
		/// <returns>A list of Service Item pairs between which a relationship will be built. </returns>
		public IEnumerable<(ServiceItemsSection, ServiceItemsSection)> ToSequentialPairs(IEnumerable<ServiceItemsSection> source)
		{
			using (var enumerator = source.GetEnumerator())
			{
				if (!enumerator.MoveNext())
					yield break;

				var previous = enumerator.Current;
				while (enumerator.MoveNext())
				{
					yield return (previous, enumerator.Current);
					previous = enumerator.Current;
				}
			}
		}

		public List<ServiceItemRelationshipSection> FindRelationshipsBetweenPair(
			IServiceInstanceBase instance,
			(ServiceItemsSection, ServiceItemsSection) pair)
		{
			var relationships = GetRelationships(instance);
			var parentId = pair.Item1.ServiceItemID.ToString();
			var childId = pair.Item2.ServiceItemID.ToString();

			return relationships.Where(r =>
				r.ParentServiceItem == parentId &&
				r.ChildServiceItem == childId).ToList();
		}

		public IServiceInstanceBase GetDomInstance(Guid domId)
		{
			var instance = _smDomHelper.DomInstances
				.Read(DomInstanceExposers.Id.Equal(domId))
				.FirstOrDefault();

			if (instance == null)
				throw new InvalidOperationException($"Could not find the DOM instance with id {domId}");

			return ServiceInstancesExtentions.GetTypedInstance(instance);
		}

		public IEnumerable<ServiceItemsSection> GetServiceItems(
			IServiceInstanceBase instance,
			IEnumerable<string> serviceItemIds)
		{
			var items = instance.GetServiceItems();
			return GetServiceItemsFromList(items, serviceItemIds);
		}

		public void Update(List<ServiceItemLinkMap> linkMap, IServiceInstanceBase instance)
		{
			var relationships = GetRelationshipsMutable(instance);

			foreach (var link in linkMap.SelectMany(pair => pair.Links))
			{
				var existing = relationships
					.FirstOrDefault(r => r.ParentServiceItem == link.ParentServiceItem &&
					r.ChildServiceItem == link.ChildServiceItem &&
					r.ParentServiceItemInterfaceID == link.ParentServiceItemInterfaceID);

				if (existing != null)
					relationships.Remove(existing);

				if (!string.IsNullOrEmpty(link.ChildServiceItemInterfaceID))
					relationships.Add(link);
			}

			instance.Save(_smDomHelper);
		}

		public string CreateServiceItem(Guid domId, string definitionReference, string type)
		{
			var addServiceItemScript = _engine.PrepareSubScript("SLC_SM_AS_AddServiceItem");
			addServiceItemScript.SelectScriptParam("DOM ID", $"[\"{domId}\"]");
			addServiceItemScript.SelectScriptParam("ServiceItemType", type);
			addServiceItemScript.SelectScriptParam("DefinitionReference", definitionReference);
			addServiceItemScript.Synchronous = true;
			addServiceItemScript.InheritScriptOutput = true;
			addServiceItemScript.StartScript();

			if (addServiceItemScript.HadError)
				throw new InvalidOperationException($"Error creating the service item:{addServiceItemScript.GetErrorMessages()}");

			return _engine.GetScriptOutput("ServiceItemId");
		}

		public IDefinitionObject ResolveDefinitionReference(IServiceInstanceBase instance, ServiceItemsSection serviceItem)
		{
			var existingRelationships = GetRelationships(instance);
			if (serviceItem.ServiceItemType.Value == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Workflow)
			{
				return new WorkflowsInstanceAdapter(serviceItem, GetWorkflowbyName(serviceItem.DefinitionReference), existingRelationships);
			}

			if (serviceItem.ServiceItemType.Value == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.SRMBooking)
			{
				return new SRMBooking(serviceItem, existingRelationships);
			}

			if (serviceItem.ServiceItemType.Value == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Service)
			{
				return new ServiceLink(instance);
			}

			throw new ArgumentException($"Unknown definition reference: {serviceItem.DefinitionReference}");
		}

		private IEnumerable<ServiceItemsSection> GetServiceItemsFromList(
			IEnumerable<ServiceItemsSection> serviceItems,
			IEnumerable<string> serviceItemIds)
		{
			var itemsById = serviceItems
				.Where(n => n.ServiceItemID.HasValue)
				.ToDictionary(n => n.ServiceItemID.Value.ToString());

			return serviceItemIds
				.Select(id => itemsById.TryGetValue(id, out var item) ? item : null)
				.Where(item => item != null);
		}

		private IList<ServiceItemRelationshipSection> GetRelationshipsMutable(IServiceInstanceBase instance)
		{
			return instance.GetServiceItemRelationships();
		}

		private IList<ServiceItemRelationshipSection> GetRelationships(IServiceInstanceBase instance)
		{
			return GetRelationshipsMutable(instance).ToList(); // Copy
		}
	}
}