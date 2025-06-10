namespace SLC_SM_Common.API.PeopleAndOrganizationApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcPeople_Organizations;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class DataHelperCategory : DataHelper<Models.Category>
	{
		public DataHelperCategory(IConnection connection) : base(connection, SlcPeople_OrganizationsIds.Definitions.Category)
		{
		}

		public override List<Models.Category> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new CategoryInstance(x))
				.ToList();

			return instances.Select(
					x => new Models.Category
					{
						ID = x.ID.Id,
						Name = x.CategoryInformation.Category,
					})
				.ToList();
		}

		public override Guid CreateOrUpdate(Models.Category item)
		{
			var instance = new CategoryInstance(New(item.ID));
			instance.CategoryInformation.Category = item.Name;

			return CreateOrUpdateInstance(instance);
		}
	}
}