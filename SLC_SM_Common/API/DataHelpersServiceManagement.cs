namespace Library
{
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;

	/// <summary>
	///     Provides access to data helper classes for service management entities.
	/// </summary>
	public sealed class DataHelpersServiceManagement
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="DataHelpersServiceManagement" /> class.
		/// </summary>
		/// <param name="connection">The connection to use for data operations.</param>
		public DataHelpersServiceManagement(IConnection connection)
		{
			ServiceCategories = new DataHelperServiceCategory(connection);
			ServiceSpecifications = new DataHelperServiceSpecification(connection);
			ServiceProperties = new DataHelperServiceProperties(connection);
			ServicePropertyValues = new DataHelperServicePropertyValues(connection);
			ServiceOrders = new DataHelperServiceOrder(connection);
			ServiceOrderItems = new DataHelperServiceOrderItem(connection);
			Services = new DataHelperService(connection);
			ServiceOrderItemConfigurationValues = new DataHelperServiceOrderItemConfigurationValue(connection);
			ServiceSpecificationConfigurationValues = new DataHelperServiceSpecificationConfigurationValue(connection);
			ServiceConfigurationValues = new DataHelperServiceConfigurationValue(connection);
		}

		/// <summary>
		///     Gets the data helper for service orders.
		/// </summary>
		public DataHelperServiceOrder ServiceOrders { get; }

		/// <summary>
		///     Gets the data helper for service order items.
		/// </summary>
		public DataHelperServiceOrderItem ServiceOrderItems { get; }

		/// <summary>
		///     Gets the data helper for service specifications.
		/// </summary>
		public DataHelperServiceSpecification ServiceSpecifications { get; }

		/// <summary>
		///     Gets the data helper for service properties.
		/// </summary>
		public DataHelperServiceProperties ServiceProperties { get; }

		/// <summary>
		///     Gets the data helper for service property values.
		/// </summary>
		public DataHelperServicePropertyValues ServicePropertyValues { get; }

		/// <summary>
		///     Gets the data helper for services.
		/// </summary>
		public DataHelperService Services { get; }

		/// <summary>
		///     Gets the data helper for service categories.
		/// </summary>
		public DataHelperServiceCategory ServiceCategories { get; }

		/// <summary>
		///     Gets the data helper for service configuration values.
		/// </summary>
		public DataHelperServiceConfigurationValue ServiceConfigurationValues { get; }

		/// <summary>
		///     Gets the data helper for service specification configuration values.
		/// </summary>
		public DataHelperServiceSpecificationConfigurationValue ServiceSpecificationConfigurationValues { get; }

		/// <summary>
		///     Gets the data helper for service order item configuration values.
		/// </summary>
		public DataHelperServiceOrderItemConfigurationValue ServiceOrderItemConfigurationValues { get; }
	}
}