namespace Library
{
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization;

	/// <summary>
	///     Provides helper classes for managing categories, organizations, people, and personal experiences.
	/// </summary>
	public class DataHelpersPeopleAndOrganizations
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="DataHelpersPeopleAndOrganizations" /> class.
		/// </summary>
		/// <param name="connection">The connection to use for data operations.</param>
		public DataHelpersPeopleAndOrganizations(IConnection connection)
		{
			Categories = new DataHelperCategory(connection);
			Organizations = new DataHelperOrganization(connection);
			People = new DataHelperPeople(connection);
			Experiences = new DataHelperPersonalExperience(connection);
		}

		/// <summary>
		///     Gets the category data helper.
		/// </summary>
		public DataHelperCategory Categories { get; }

		/// <summary>
		///     Gets the organization data helper.
		/// </summary>
		public DataHelperOrganization Organizations { get; }

		/// <summary>
		///     Gets the people data helper.
		/// </summary>
		public DataHelperPeople People { get; }

		/// <summary>
		///     Gets the personal experience data helper.
		/// </summary>
		public DataHelperPersonalExperience Experiences { get; }
	}
}