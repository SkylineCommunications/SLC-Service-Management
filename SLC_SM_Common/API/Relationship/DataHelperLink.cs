namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.Relationship
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcRelationships;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class DataHelperLink : DataHelper<Models.Link>
	{
		public DataHelperLink(IConnection connection) : base(connection, SlcRelationshipsIds.Definitions.Links)
		{
		}

		public override Guid CreateOrUpdate(Models.Link item)
		{
			DomInstance domInstance = New(item.ID);
			var instance = new LinksInstance(domInstance);
			instance.LinkInfo.ChildObjectID = item.ChildID;
			instance.LinkInfo.ChildObjectName = item.ChildName;
			instance.LinkInfo.ChildObjectType = Guid.Empty;
			instance.LinkInfo.ParentObjectID = item.ParentID;
			instance.LinkInfo.ParentObjectName = item.ParentName;
			instance.LinkInfo.ParentObjectType = Guid.Empty;

			return CreateOrUpdateInstance(instance);
		}

		public override List<Models.Link> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id));
			return Read(instances);
		}

		public override bool TryDelete(Models.Link item)
		{
			return TryDelete(item.ID);
		}

		private List<Models.Link> Read(List<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new LinksInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.Link>();
			}

			return instances.Select(
					x => new Models.Link
					{
						ID = x.ID.Id,
						ChildID = x.LinkInfo.ChildObjectID,
						ChildName = x.LinkInfo.ChildObjectName,
						ParentID = x.LinkInfo.ParentObjectID,
						ParentName = x.LinkInfo.ParentObjectName,
					})
				.ToList();
		}
	}
}