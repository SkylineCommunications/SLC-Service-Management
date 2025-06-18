namespace SLC_SM_Common.API.ConfigurationsApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcConfigurations;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class DataHelperDiscreteValues : DataHelper<Models.DiscreteValue>
	{
		public DataHelperDiscreteValues(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.DiscreteValues)
		{
		}

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

		public override Guid CreateOrUpdate(Models.DiscreteValue item)
		{
			var instance = new DiscreteValuesInstance(New(item.ID));
			instance.DiscreteValue.Value = item.Value;

			return CreateOrUpdateInstance(instance);
		}

		public override bool TryDelete(Models.DiscreteValue item)
		{
			return TryDelete(item.ID);
		}
	}
}