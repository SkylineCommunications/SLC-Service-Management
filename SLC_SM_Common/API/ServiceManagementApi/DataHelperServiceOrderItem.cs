namespace SLC_SM_Common.API.ServiceManagementApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class DataHelperServiceOrderItem : DataHelper<Models.ServiceOrderItem>
	{
		public DataHelperServiceOrderItem(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.ServiceOrderItems)
		{
		}

		public override List<Models.ServiceOrderItem> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new ServiceOrderItemsInstance(x))
				.ToList();

			var helperPropertyValues = new DataHelperServicePropertyValues(_connection);
			var helperConfigurations = new DataHelperServiceOrderItemConfigurationValue(_connection);
			return instances.Select(
					x => new Models.ServiceOrderItem
					{
						ID = x.ID.Id,
						Name = x.ServiceOrderItemInfo.Name,
						Action = x.ServiceOrderItemInfo.Action,
						StartTime = x.ServiceOrderItemInfo.ServiceStartTime,
						EndTime = x.ServiceOrderItemInfo.ServiceEndTime,
						IndefiniteRuntime = x.ServiceOrderItemInfo.ServiceIndefiniteRuntime,
						SpecificationId = x.ServiceOrderItemServiceInfo.ServiceSpecification,
						ServiceCategoryId = x.ServiceOrderItemServiceInfo.ServiceCategory,
						Properties = helperPropertyValues.Read().Find(p => p.ID == x.ServiceOrderItemServiceInfo.Properties),
						Configurations = helperConfigurations.Read().Where(c => x.ServiceOrderItemServiceInfo.ServiceOrderItemConfigurations.Contains(c.ID)).ToList(),
						ServiceId = default, // TODO
					})
				.ToList();
		}

		public override Guid CreateOrUpdate(Models.ServiceOrderItem item)
		{
			var instance = new ServiceOrderItemsInstance(New(item.ID));
			instance.ServiceOrderItemInfo.Name = item.Name;
			instance.ServiceOrderItemInfo.Action = item.Action;
			instance.ServiceOrderItemInfo.ServiceStartTime = item.StartTime;
			instance.ServiceOrderItemInfo.ServiceEndTime = item.EndTime;
			instance.ServiceOrderItemInfo.ServiceIndefiniteRuntime = item.IndefiniteRuntime;

			instance.ServiceOrderItemServiceInfo.ServiceSpecification = item.SpecificationId;
			instance.ServiceOrderItemServiceInfo.ServiceCategory = item.ServiceCategoryId;
			instance.ServiceOrderItemServiceInfo.Properties = item.Properties?.ID;
			instance.ServiceOrderItemServiceInfo.Service = item.ServiceCategoryId;

			if (item.Properties != null)
			{
				var dataHelperProperties = new DataHelperServicePropertyValues(_connection);
				dataHelperProperties.CreateOrUpdate(item.Properties);
			}

			var dataHelperConfigurations = new DataHelperServiceOrderItemConfigurationValue(_connection);
			foreach (var configurationValue in item.Configurations)
			{
				instance.ServiceOrderItemServiceInfo.ServiceOrderItemConfigurations.Add(configurationValue.ID);
				dataHelperConfigurations.CreateOrUpdate(configurationValue);
			}

			return CreateOrUpdateInstance(instance);
		}
	}
}