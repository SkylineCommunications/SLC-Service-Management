namespace Library
{
	using System.Collections.Generic;

	using DomHelpers.SlcConfigurations;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	public class RepoConfigurations
	{
		public RepoConfigurations(DomHelper helper)
		{
			ConfigurationParameters = helper.GetConfigurationParameterInstances();
			NumberParameterOptions = helper.GetNumberParameterOptionsInstances();
			DiscreteParameterOptions = helper.GetDiscreteParameterOptions();
			TextParameterOptions = helper.GetTextParameterOptions();
			DiscreteValues = helper.GetDiscreteValues();
			ConfigurationUnits = helper.GetConfigurationUnits();
		}

		public List<ConfigurationParametersInstance> ConfigurationParameters { get; }

		public List<NumberParameterOptionsInstance> NumberParameterOptions { get; }

		public List<DiscreteParameterOptionsInstance> DiscreteParameterOptions { get; }

		public List<TextParameterOptionsInstance> TextParameterOptions { get; }

		public List<DiscreteValuesInstance> DiscreteValues { get; }

		public List<ConfigurationUnitInstance> ConfigurationUnits { get; }
	}
}