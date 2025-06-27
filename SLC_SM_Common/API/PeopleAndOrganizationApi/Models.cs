namespace SLC_SM_Common.API
{
	using System;

	namespace PeopleAndOrganizationApi
	{
		public static class Models
		{
			public class Organization
			{
				public Guid ID { get; set; }

				public Guid? CategoryId { get; set; }

				public string Name { get; set; }
			}

			public class People
			{
				public Guid ID { get; set; }

				public string FullName { get; set; }

				public Guid? OrganizationId { get; set; }

				public string Mail { get; set; }

				public string Phone { get; set; }

				public string Skill { get; set; }

				public ExperienceLevel ExperienceLevel { get; set; }
			}

			public class ExperienceLevel
			{
				public Guid ID { get; set; }

				public string Value { get; set; }
			}

			public class Category
			{
				public Guid ID { get; set; }

				public string Name { get; set; }
			}
		}
	}
}