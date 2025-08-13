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
	public class DataHelperDiscreteValues : DataHelper<Models.DiscreteValue>
	{
		/// <inheritdoc />
		public DataHelperDiscreteValues(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.DiscreteValues)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.DiscreteValue item)
		{
			var instance = new DiscreteValuesInstance(New(item.ID));
			instance.DiscreteValue.Value = item.Value;

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override List<Models.DiscreteValue> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new DiscreteValuesInstance(x))
				.ToList();

			return instances.Select(
					x => new Models.DiscreteValue
					{
						ID = x.ID.Id,
						Value = x.DiscreteValue.Value,
					})
				.ToList();
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.DiscreteValue item)
		{
			return TryDelete(item.ID);
		}
	}
}