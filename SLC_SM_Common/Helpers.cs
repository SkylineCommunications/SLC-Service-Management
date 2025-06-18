namespace Library
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcConfigurations;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public static class Helpers
	{
		public static ConfigurationParametersInstance GetConfigurationParameterInstance(this DomHelper helper, Guid? reference)
		{
			var value = GetDomInstance(helper, reference);
			if (value == null)
			{
				return default;
			}

			return new ConfigurationParametersInstance(value);
		}

		public static List<ConfigurationParametersInstance> GetConfigurationParameterInstances(this DomHelper helper)
		{
			return helper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcConfigurationsIds.Definitions.ConfigurationParameters.Id))
				.Select(x => new ConfigurationParametersInstance(x))
				.ToList();
		}

		public static ConfigurationParameterValueInstance GetConfigurationParameterValueInstance(this DomHelper helper, Guid? reference)
		{
			var value = GetDomInstance(helper, reference);
			if (value == null)
			{
				return default;
			}

			return new ConfigurationParameterValueInstance(value);
		}

		public static List<ConfigurationUnitInstance> GetConfigurationUnits(this DomHelper helper)
		{
			return helper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcConfigurationsIds.Definitions.ConfigurationUnit.Id))
				.Select(x => new ConfigurationUnitInstance(x))
				.ToList();
		}

		public static List<DiscreteParameterOptionsInstance> GetDiscreteParameterOptions(this DomHelper helper)
		{
			return helper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcConfigurationsIds.Definitions.DiscreteParameterOptions.Id))
				.Select(x => new DiscreteParameterOptionsInstance(x))
				.ToList();
		}

		public static DiscreteParameterOptionsInstance GetDiscreteParameterOptionsInstance(this DomHelper helper, Guid? reference)
		{
			var value = GetDomInstance(helper, reference);
			if (value == null)
			{
				return default;
			}

			return new DiscreteParameterOptionsInstance(value);
		}

		public static List<DiscreteValuesInstance> GetDiscreteValues(this DomHelper helper)
		{
			return helper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcConfigurationsIds.Definitions.DiscreteValues.Id))
				.Select(x => new DiscreteValuesInstance(x))
				.ToList();
		}

		public static List<DiscreteValuesInstance> GetDiscreteValuesInstances(this DomHelper helper, DiscreteParameterOptionsInstance reference)
		{
			return GetDomInstances(helper, reference.DiscreteParameterOptions.DiscreteValues)
				.Select(x => new DiscreteValuesInstance(x))
				.ToList();
		}

		public static DomInstance GetDomInstance(this DomHelper helper, Guid? reference)
		{
			if (!reference.HasValue)
			{
				return default;
			}

			return helper.DomInstances.Read(DomInstanceExposers.Id.Equal(reference.Value)).FirstOrDefault();
		}

		public static List<DomInstance> GetDomInstances(this DomHelper helper, IList<Guid> references)
		{
			if (!references.Any())
			{
				return new List<DomInstance>();
			}

			FilterElement<DomInstance> filter = new ORFilterElement<DomInstance>();
			foreach (Guid reference in references)
			{
				filter = filter.OR(DomInstanceExposers.Id.Equal(reference));
			}

			return helper.DomInstances.Read(filter);
		}

		public static List<NumberParameterOptionsInstance> GetNumberParameterOptionsInstances(this DomHelper helper)
		{
			return helper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcConfigurationsIds.Definitions.NumberParameterOptions.Id))
				.Select(x => new NumberParameterOptionsInstance(x))
				.ToList();
		}

		public static List<TextParameterOptionsInstance> GetTextParameterOptions(this DomHelper helper)
		{
			return helper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcConfigurationsIds.Definitions.TextParameterOptions.Id))
				.Select(x => new TextParameterOptionsInstance(x))
				.ToList();
		}
	}
}