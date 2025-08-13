namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <inheritdoc />
	public class DataHelperServicePropertyValues : DataHelper<Models.ServicePropertyValues>
	{
		/// <inheritdoc />
		public DataHelperServicePropertyValues(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.ServicePropertyValues)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ServicePropertyValues item)
		{
			var instance = new ServicePropertyValuesInstance(New(item.ID));

			if (item.Values != null)
			{
				foreach (var value in item.Values)
				{
					instance.ServicePropertyValue.Add(
						new ServicePropertyValueSection
						{
							Value = value.Value,
							Property = value.ServicePropertyId,
						});
				}
			}

			if (!instance.ServicePropertyValue.Any())
			{
				instance.ServicePropertyValue.Add(new ServicePropertyValueSection());
			}

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override List<Models.ServicePropertyValues> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new ServicePropertyValuesInstance(x))
				.ToList();

			return instances.Select(
					x => new Models.ServicePropertyValues
					{
						ID = x.ID.Id,
						Values = x.ServicePropertyValue.Select(
								p => new Models.ServicePropertyValue
								{
									Value = p.Value,
									ServicePropertyId = p.Property ?? Guid.Empty,
								})
							.ToList(),
					})
				.ToList();
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.ServicePropertyValues item)
		{
			return TryDelete(item.ID);
		}
	}
}