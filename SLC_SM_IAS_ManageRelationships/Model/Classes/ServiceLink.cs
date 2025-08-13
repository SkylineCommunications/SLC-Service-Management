namespace SLC_SM_IAS_ManageRelationships.Controller
{
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;
	using DomHelpers.SlcWorkflow;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.Relationship;

	internal class ServiceLink : IDefinitionObject
	{
		private readonly List<Models.Link> _existingRelationships;
		private readonly IServiceInstanceBase _serviceItem;

		public ServiceLink(IServiceInstanceBase serviceInfo)
		{
			_serviceItem = serviceInfo;
			_existingRelationships = new DataHelperLink(Engine.SLNetRaw).Read();
		}

		IEnumerable<NodesSection> IDefinitionObject.GetAvailableInputs()
		{
			return _existingRelationships.Any(r => r.ChildID == _serviceItem.GetId().Id.ToString())
				? Enumerable.Empty<NodesSection>()
				: new[] { new NodesSection { NodeID = "0", NodeAlias = "Default Service Link Input" } };
		}

		IEnumerable<NodesSection> IDefinitionObject.GetAvailableOutputs()
		{
			return _existingRelationships.Any(r => r.ParentID == _serviceItem.GetId().Id.ToString())
				? Enumerable.Empty<NodesSection>()
				: new[] { new NodesSection { NodeID = "1", NodeAlias = "Default Service Link Output" } };
		}
	}
}