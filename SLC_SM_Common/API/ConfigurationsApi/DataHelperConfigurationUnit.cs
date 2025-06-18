namespace SLC_SM_Common.API.ConfigurationsApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcConfigurations;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class DataHelperConfigurationUnit : DataHelper<Models.ConfigurationUnit>
	{
		public DataHelperConfigurationUnit(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.ConfigurationUnit)
		{
		}

		public override List<Models.ConfigurationUnit> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new ConfigurationUnitInstance(x))
				.ToList();

			return instances.Select(
					x => new Models.ConfigurationUnit
					{
						ID = x.ID.Id,
						Name = x.ConfigurationUnitInfo.UnitName,
					})
				.ToList();
		}

		public override Guid CreateOrUpdate(Models.ConfigurationUnit item)
		{
			var instance = new ConfigurationUnitInstance(New(item.ID));
			instance.ConfigurationUnitInfo.UnitName = item.Name;

			return CreateOrUpdateInstance(instance);
		}

		public override bool TryDelete(Models.ConfigurationUnit item)
		{
			return TryDelete(item.ID);
		}
	}
}