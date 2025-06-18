namespace Library
{
	using Skyline.DataMiner.Net;
	using SLC_SM_Common.API.ServiceManagementApi;

	public sealed class Repo
	{
		public Repo(IConnection connection)
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

		public DataHelperServiceOrder ServiceOrders { get; }

		public DataHelperServiceOrderItem ServiceOrderItems { get; }

		public DataHelperServiceSpecification ServiceSpecifications { get; }

		public DataHelperServiceProperties ServiceProperties { get; }

		public DataHelperServicePropertyValues ServicePropertyValues { get; }

		public DataHelperService Services { get; }

		public DataHelperServiceCategory ServiceCategories { get; }

		public DataHelperServiceConfigurationValue ServiceConfigurationValues { get; }

		public DataHelperServiceSpecificationConfigurationValue ServiceSpecificationConfigurationValues { get; }

		public DataHelperServiceOrderItemConfigurationValue ServiceOrderItemConfigurationValues { get; }
	}
}