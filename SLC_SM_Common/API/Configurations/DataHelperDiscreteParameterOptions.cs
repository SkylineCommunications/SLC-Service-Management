namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcConfigurations;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <inheritdoc />
	public class DataHelperDiscreteParameterOptions : DataHelper<Models.DiscreteParameterOptions>
	{
		/// <inheritdoc />
		public DataHelperDiscreteParameterOptions(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.DiscreteParameterOptions)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.DiscreteParameterOptions item)
		{
			var instance = new DiscreteParameterOptionsInstance(New(item.ID));

			var dataHelperDiscreteValues = new DataHelperDiscreteValues(_connection);
			var discretes = dataHelperDiscreteValues.Read();

			if (item.Default != null)
			{
				var value = discretes.Find(d => d.ID == item.ID || d.Value == item.Default.Value);
				if (value == null)
				{
					item.Default.ID = dataHelperDiscreteValues.CreateOrUpdate(item.Default);
				}
				else
				{
					item.Default.ID = value.ID;
				}

				instance.DiscreteParameterOptions.DefaultDiscreteValue = item.Default.ID;
				discretes = dataHelperDiscreteValues.Read();
			}

			if (item.DiscreteValues != null)
			{
				foreach (var discreteValue in item.DiscreteValues)
				{
					var value = discretes.Find(d => d.ID == discreteValue.ID || d.Value == discreteValue.Value);
					if (value == null)
					{
						discreteValue.ID = dataHelperDiscreteValues.CreateOrUpdate(discreteValue);
					}
					else
					{
						discreteValue.ID = value.ID;
					}

					instance.DiscreteParameterOptions.DiscreteValues.Add(discreteValue.ID);
				}
			}

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override List<Models.DiscreteParameterOptions> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new DiscreteParameterOptionsInstance(x))
				.ToList();

			var dataHelperDiscreteValues = new DataHelperDiscreteValues(_connection);
			var discretes = dataHelperDiscreteValues.Read();

			return instances.Select(
					x => new Models.DiscreteParameterOptions
					{
						ID = x.ID.Id,
						Default = discretes.Find(d => d.ID == x.DiscreteParameterOptions.DefaultDiscreteValue),
						DiscreteValues = discretes.Where(d => x.DiscreteParameterOptions.DiscreteValues?.Contains(d.ID) == true).ToList(),
					})
				.ToList();
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.DiscreteParameterOptions item)
		{
			return TryDelete(item.ID);
		}
	}
}