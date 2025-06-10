namespace SLC_SM_Common.API.PeopleAndOrganizationApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcPeople_Organizations;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class DataHelperPersonalExperience : DataHelper<Models.ExperienceLevel>
	{
		public DataHelperPersonalExperience(IConnection connection) : base(connection, SlcPeople_OrganizationsIds.Definitions.Experience)
		{
		}

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

		public override Guid CreateOrUpdate(Models.ExperienceLevel item)
		{
			var instance = new ExperienceInstance(New(item.ID));
			instance.ExperienceInformation.Experience = item.Value;

			return CreateOrUpdateInstance(instance);
		}
	}
}