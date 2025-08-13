namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcPeople_Organizations;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <inheritdoc />
	public class DataHelperCategory : DataHelper<Models.Category>
	{
		/// <inheritdoc />
		public DataHelperCategory(IConnection connection) : base(connection, SlcPeople_OrganizationsIds.Definitions.Category)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.Category item)
		{
			var instance = new CategoryInstance(New(item.ID));
			instance.CategoryInformation.Category = item.Name;

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
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

		/// <inheritdoc />
		public override bool TryDelete(Models.Category item)
		{
			return TryDelete(item.ID);
		}
	}
}