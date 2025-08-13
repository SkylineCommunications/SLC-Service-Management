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
	public class DataHelperPersonalExperience : DataHelper<Models.ExperienceLevel>
	{
		/// <inheritdoc />
		public DataHelperPersonalExperience(IConnection connection) : base(connection, SlcPeople_OrganizationsIds.Definitions.Experience)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ExperienceLevel item)
		{
			var instance = new ExperienceInstance(New(item.ID));
			instance.ExperienceInformation.Experience = item.Value;

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override List<Models.ExperienceLevel> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new ExperienceInstance(x))
				.ToList();

			return instances.Select(
					x => new Models.ExperienceLevel
					{
						ID = x.ID.Id,
						Value = x.ExperienceInformation.Experience,
					})
				.ToList();
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.ExperienceLevel item)
		{
			return TryDelete(item.ID);
		}
	}
}