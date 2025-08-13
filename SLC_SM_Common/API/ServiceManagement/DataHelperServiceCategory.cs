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
	public class DataHelperServiceCategory : DataHelper<Models.ServiceCategory>
	{
		/// <inheritdoc />
		public DataHelperServiceCategory(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.ServiceCategory)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ServiceCategory item)
		{
			var instance = new ServiceCategoryInstance(New(item.ID));
			instance.ServiceCategoryInfo.Name = item.Name;
			instance.ServiceCategoryInfo.Type = item.Type;

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
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

		/// <inheritdoc />
		public override bool TryDelete(Models.ServiceCategory item)
		{
			return TryDelete(item.ID);
		}
	}
}