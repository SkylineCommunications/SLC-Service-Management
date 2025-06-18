namespace SLC_SM_Common.API.ConfigurationsApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcConfigurations;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class DataHelperNumberParameterOptions : DataHelper<Models.NumberParameterOptions>
	{
		public DataHelperNumberParameterOptions(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.NumberParameterOptions)
		{
		}

		public override List<Models.NumberParameterOptions> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new NumberParameterOptionsInstance(x))
				.ToList();

			var dataHelperUnits = new DataHelperConfigurationUnit(_connection);
			var units = dataHelperUnits.Read();

			return instances.Select(
					x => new Models.NumberParameterOptions
					{
						ID = x.ID.Id,
						Units = units.Where(u => x.NumberParameterOptions.Units?.Contains(u.ID) == true).ToList(),
						DefaultUnit = units.Find(u => u.ID == x.NumberParameterOptions.DefaultUnit),
						MinRange = x.NumberParameterOptions.MinRange,
						MaxRange = x.NumberParameterOptions.MaxRange,
						StepSize = x.NumberParameterOptions.StepSize,
						Decimals = x.NumberParameterOptions.Decimals,
						DefaultValue = x.NumberParameterOptions.DefaultValue,
					})
				.ToList();
		}

		public override Guid CreateOrUpdate(Models.NumberParameterOptions item)
		{
			var instance = new NumberParameterOptionsInstance(New(item.ID));
			instance.NumberParameterOptions.MinRange = item.MinRange;
			instance.NumberParameterOptions.MaxRange = item.MaxRange;
			instance.NumberParameterOptions.StepSize = item.StepSize;
			instance.NumberParameterOptions.Decimals = item.Decimals;
			instance.NumberParameterOptions.DefaultValue = item.DefaultValue;

			var dataHelperUnits = new DataHelperConfigurationUnit(_connection);
			var units = dataHelperUnits.Read();

			if (item.DefaultUnit != null)
			{
				var value = units.Find(u => u.ID == item.DefaultUnit.ID || u.Name == item.DefaultUnit.Name);
				if (value == null)
				{
					item.DefaultUnit.ID = dataHelperUnits.CreateOrUpdate(item.DefaultUnit);
				}
				else
				{
					item.DefaultUnit.ID = value.ID;
				}

				instance.NumberParameterOptions.DefaultUnit = item.DefaultUnit.ID;
				units = dataHelperUnits.Read();
			}

			if (item.Units != null)
			{
				foreach (var unit in item.Units)
				{
					var value = units.Find(u => u.ID == unit.ID || u.Name == unit.Name);
					if (value == null)
					{
						unit.ID = dataHelperUnits.CreateOrUpdate(unit);
					}
					else
					{
						unit.ID = value.ID;
					}

					instance.NumberParameterOptions.Units.Add(unit.ID);
				}
			}

			return CreateOrUpdateInstance(instance);
		}

		public override bool TryDelete(Models.NumberParameterOptions item)
		{
			return TryDelete(item.ID);
		}
	}
}