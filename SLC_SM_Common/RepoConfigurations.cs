namespace Library
{
	using Skyline.DataMiner.Net;

	using SLC_SM_Common.API.ConfigurationsApi;

	public class RepoConfigurations
	{
		public RepoConfigurations(IConnection connection)
		{
			ConfigurationParameterValues = new DataHelperConfigurationParameterValue(connection);
			ConfigurationParameters = new DataHelperConfigurationParameter(connection);
			NumberParameterOptions = new DataHelperNumberParameterOptions(connection);
			DiscreteParameterOptions = new DataHelperDiscreteParameterOptions(connection);
			TextParameterOptions = new DataHelperTextParameterOptions(connection);
			DiscreteValues = new DataHelperDiscreteValues(connection);
			ConfigurationUnits = new DataHelperConfigurationUnit(connection);
		}

		public DataHelperConfigurationParameterValue ConfigurationParameterValues { get; }

		public DataHelperConfigurationParameter ConfigurationParameters { get; }

		public DataHelperNumberParameterOptions NumberParameterOptions { get; }

		public DataHelperDiscreteParameterOptions DiscreteParameterOptions { get; }

		public DataHelperTextParameterOptions TextParameterOptions { get; }

		public DataHelperDiscreteValues DiscreteValues { get; }

		public DataHelperConfigurationUnit ConfigurationUnits { get; }
	}
}