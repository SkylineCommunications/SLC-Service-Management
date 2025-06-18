namespace SLC_SM_Common.API
{
	using System;
	using System.Collections.Generic;

	using DomHelpers.SlcConfigurations;

	namespace ConfigurationsApi
	{
		public static class Models
		{
			public class ConfigurationParameterValue
			{
				public Guid ID { get; set; }

				public string Label { get; set; }

				public SlcConfigurationsIds.Enums.Type Type { get; set; }

				public double? DoubleValue { get; set; }

				public string StringValue { get; set; }

				public Guid ConfigurationParameterId { get; set; }

				public NumberParameterOptions NumberOptions { get; set; }

				public DiscreteParameterOptions DiscreteOptions { get; set; }

				public TextParameterOptions TextOptions { get; set; }

				public string LinkedConfigurationReference { get; set; }

				public bool ValueFixed { get; set; }
			}

			public class ConfigurationParameter
			{
				public Guid ID { get; set; }

				public string Name { get; set; }

				public SlcConfigurationsIds.Enums.Type Type { get; set; }

				public NumberParameterOptions NumberOptions { get; set; }

				public DiscreteParameterOptions DiscreteOptions { get; set; }

				public TextParameterOptions TextOptions { get; set; }
			}

			public class NumberParameterOptions
			{
				public Guid ID { get; set; }

				public List<ConfigurationUnit> Units { get; set; }

				public ConfigurationUnit DefaultUnit { get; set; }

				public double? MinRange { get; set; }

				public double? MaxRange { get; set; }

				public long? Decimals { get; set; }

				public double? StepSize { get; set; }

				public double? DefaultValue { get; set; }
			}

			public class ConfigurationUnit
			{
				public Guid ID { get; set; }

				public string Name { get; set; }
			}

			public class DiscreteParameterOptions
			{
				public Guid ID { get; set; }

				public List<DiscreteValue> DiscreteValues { get; set; }

				public DiscreteValue Default { get; set; }
			}

			public class DiscreteValue
			{
				public Guid ID { get; set; }

				public string Value { get; set; }
			}

			public class TextParameterOptions
			{
				public Guid ID { get; set; }

				public string Regex { get; set; }

				public string UserMessage { get; set; }

				public string Default { get; set; }
			}
		}
	}
}