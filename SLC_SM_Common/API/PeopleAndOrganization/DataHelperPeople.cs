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
	public class DataHelperPeople : DataHelper<Models.People>
	{
		/// <inheritdoc />
		public DataHelperPeople(IConnection connection) : base(connection, SlcPeople_OrganizationsIds.Definitions.People)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.People item)
		{
			var instance = new PeopleInstance(New(item.ID));
			instance.PeopleInformation.FullName = item.FullName;
			instance.PeopleInformation.PersonalSkills = item.Skill ?? String.Empty;
			instance.PeopleInformation.ExperienceLevel = item.ExperienceLevel.ID;
			instance.PeopleInformation.ProfileImage = String.Empty;
			instance.ContactInfo.Email = item.Mail ?? String.Empty;
			instance.ContactInfo.Phone = item.Phone ?? String.Empty;
			instance.ContactInfo.StreetAddress = "";
			instance.ContactInfo.City = "";
			instance.ContactInfo.Country = SlcPeople_OrganizationsIds.Enums.Country.Belgium;
			instance.ContactInfo.ZIP = "";
			instance.Organization.Organization_57695f03 = item.OrganizationId;

			var id = CreateOrUpdateInstance(instance);
			_domHelper.DomInstances.DoStatusTransition(instance.ID, SlcPeople_OrganizationsIds.Behaviors.People_Behavior.Transitions.Draft_To_Active);
			return id;
		}

		/// <inheritdoc />
		public override List<Models.People> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new PeopleInstance(x))
				.ToList();

			return instances.Select(
					x => new Models.People
					{
						ID = x.ID.Id,
						FullName = x.PeopleInformation.FullName,
						OrganizationId = x.Organization.Organization_57695f03,
						Skill = x.PeopleInformation.PersonalSkills,
						Mail = x.ContactInfo.Email ?? String.Empty,
						Phone = x.ContactInfo.Phone ?? String.Empty,
					})
				.ToList();
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.People item)
		{
			return TryDelete(item.ID);
		}
	}
}