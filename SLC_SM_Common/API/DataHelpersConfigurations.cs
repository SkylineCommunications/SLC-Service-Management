namespace Library
{
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations;

	/// <summary>
	///     Provides access to data helpers for configuration parameters and options.
	/// </summary>
	public class DataHelpersConfigurations
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="DataHelpersConfigurations" /> class.
		/// </summary>
		/// <param name="connection">The connection to use for data operations.</param>
		public DataHelpersConfigurations(IConnection connection)
		{
			ConfigurationParameterValues = new DataHelperConfigurationParameterValue(connection);
			ConfigurationParameters = new DataHelperConfigurationParameter(connection);
			NumberParameterOptions = new DataHelperNumberParameterOptions(connection);
			DiscreteParameterOptions = new DataHelperDiscreteParameterOptions(connection);
			TextParameterOptions = new DataHelperTextParameterOptions(connection);
			DiscreteValues = new DataHelperDiscreteValues(connection);
			ConfigurationUnits = new DataHelperConfigurationUnit(connection);
		}

		/// <summary>
		///     Gets the data helper for configuration parameter values.
		/// </summary>
		public DataHelperConfigurationParameterValue ConfigurationParameterValues { get; }

		/// <summary>
		///     Gets the data helper for configuration parameters.
		/// </summary>
		public DataHelperConfigurationParameter ConfigurationParameters { get; }

		/// <summary>
		///     Gets the data helper for number parameter options.
		/// </summary>
		public DataHelperNumberParameterOptions NumberParameterOptions { get; }

		/// <summary>
		///     Gets the data helper for discrete parameter options.
		/// </summary>
		public DataHelperDiscreteParameterOptions DiscreteParameterOptions { get; }

		/// <summary>
		///     Gets the data helper for text parameter options.
		/// </summary>
		public DataHelperTextParameterOptions TextParameterOptions { get; }

		/// <summary>
		///     Gets the data helper for discrete values.
		/// </summary>
		public DataHelperDiscreteValues DiscreteValues { get; }

		/// <summary>
		///     Gets the data helper for configuration units.
		/// </summary>
		public DataHelperConfigurationUnit ConfigurationUnits { get; }
	}
}