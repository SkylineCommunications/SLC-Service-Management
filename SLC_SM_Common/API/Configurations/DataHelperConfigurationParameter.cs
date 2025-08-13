namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcConfigurations;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;

	/// <inheritdoc />
	public class DataHelperConfigurationParameter : DataHelper<Models.ConfigurationParameter>
	{
		/// <inheritdoc />
		public DataHelperConfigurationParameter(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.ConfigurationParameters)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ConfigurationParameter item)
		{
			var instance = new ConfigurationParametersInstance(New(item.ID));
			instance.ConfigurationParameterInfo.ParameterName = item.Name;
			instance.ConfigurationParameterInfo.Type = item.Type;

			if (item.NumberOptions != null)
			{
				var numberOptionsHelper = new DataHelperNumberParameterOptions(_connection);
				instance.ConfigurationParameterInfo.NumberOptions = numberOptionsHelper.CreateOrUpdate(item.NumberOptions);
			}

			if (item.DiscreteOptions != null)
			{
				var discreteOptionsHelper = new DataHelperDiscreteParameterOptions(_connection);
				instance.ConfigurationParameterInfo.DiscreteOptions = discreteOptionsHelper.CreateOrUpdate(item.DiscreteOptions);
			}

			if (item.TextOptions != null)
			{
				var textOptionsHelper = new DataHelperTextParameterOptions(_connection);
				instance.ConfigurationParameterInfo.TextOptions = textOptionsHelper.CreateOrUpdate(item.TextOptions);
			}

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public List<Models.ConfigurationParameter> Read(FilterElement<Models.ConfigurationParameter> filter)
		{
			if (filter is null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			var domFilter = FilterTranslator.TranslateFullFilter(filter);
			return Read(_domHelper.DomInstances.Read(domFilter));
		}

		/// <inheritdoc />
		public override List<Models.ConfigurationParameter> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id));
			return Read(instances);
		}

		/// <inheritdoc />
		private List<Models.ConfigurationParameter> Read(List<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new ConfigurationParametersInstance(x)).ToList();

			var numberOptions = new DataHelperNumberParameterOptions(_connection).Read();
			var discreteOptions = new DataHelperDiscreteParameterOptions(_connection).Read();
			var textOptions = new DataHelperTextParameterOptions(_connection).Read();

			return instances.Select(
					x => new Models.ConfigurationParameter
					{
						ID = x.ID.Id,
						Name = x.ConfigurationParameterInfo.ParameterName,
						Type = x.ConfigurationParameterInfo.Type ?? SlcConfigurationsIds.Enums.Type.Text,
						NumberOptions = numberOptions.Find(o => o.ID == x.ConfigurationParameterInfo.NumberOptions),
						DiscreteOptions = discreteOptions.Find(o => o.ID == x.ConfigurationParameterInfo.DiscreteOptions),
						TextOptions = textOptions.Find(o => o.ID == x.ConfigurationParameterInfo.TextOptions),
					})
				.ToList();
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.ConfigurationParameter item)
		{
			return TryDelete(item.ID);
		}
	}
}