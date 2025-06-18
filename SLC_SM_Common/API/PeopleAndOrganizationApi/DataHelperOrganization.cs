namespace SLC_SM_Common.API.PeopleAndOrganizationApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcPeople_Organizations;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class DataHelperOrganization : DataHelper<Models.Organization>
	{
		public DataHelperOrganization(IConnection connection) : base(connection, SlcPeople_OrganizationsIds.Definitions.Organizations)
		{
		}

		public override List<Models.Organization> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new OrganizationsInstance(x))
				.ToList();

			return instances.Select(
					x => new Models.Organization
					{
						ID = x.ID.Id,
						Name = x.OrganizationInformation.OrganizationName,
						CategoryId = x.OrganizationInformation.Category.Value,
					})
				.ToList();
		}

		public override Guid CreateOrUpdate(Models.Organization item)
		{
			var instance = new OrganizationsInstance(New(item.ID));
			instance.OrganizationInformation.OrganizationName = item.Name;
			instance.OrganizationInformation.Category = item.CategoryId;

			return CreateOrUpdateInstance(instance);
		}

		public override bool TryDelete(Models.Organization item)
		{
			return TryDelete(item.ID);
		}
	}
}