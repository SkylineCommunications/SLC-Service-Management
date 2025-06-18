namespace SLC_SM_Common.API.ServiceManagementApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class DataHelperServiceCategory : DataHelper<Models.ServiceCategory>
	{
		public DataHelperServiceCategory(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.ServiceCategory)
		{
		}

		public override List<Models.ServiceCategory> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new ServiceCategoryInstance(x))
				.ToList();

			return instances.Select(
					x => new Models.ServiceCategory
					{
						ID = x.ID.Id,
						Name = x.ServiceCategoryInfo.Name,
						Type = x.ServiceCategoryInfo.Type,
					})
				.ToList();
		}

		public override Guid CreateOrUpdate(Models.ServiceCategory item)
		{
			var instance = new ServiceCategoryInstance(New(item.ID));
			instance.ServiceCategoryInfo.Name = item.Name;
			instance.ServiceCategoryInfo.Type = item.Type;

			return CreateOrUpdateInstance(instance);
		}

		public override bool TryDelete(Models.ServiceCategory item)
		{
			return TryDelete(item.ID);
		}
	}
}