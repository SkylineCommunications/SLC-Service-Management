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
	public class DataHelperServiceOrder : DataHelper<Models.ServiceOrder>
	{
		/// <inheritdoc />
		public DataHelperServiceOrder(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.ServiceOrders)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ServiceOrder order)
		{
			DomInstance domInstance = New(order.ID);
			var existingStatusId = _domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(domInstance.ID)).FirstOrDefault()?.StatusId;
			if (existingStatusId != null)
			{
				domInstance.StatusId = existingStatusId;
			}

			var instance = new ServiceOrdersInstance(domInstance);
			instance.ServiceOrderInfo.Name = order.Name;
			instance.ServiceOrderInfo.Description = order.Description;
			instance.ServiceOrderInfo.ExternalID = order.ExternalID;
			instance.ServiceOrderInfo.Priority = order.Priority;

			if (!String.IsNullOrEmpty(order.OrderId))
			{
				instance.ServiceOrderInfo.ID = order.OrderId;
			}

			instance.ServiceOrderInfo.RelatedOrganization = order.OrganizationId;

			instance.ServiceOrderInfo.OrderContact.Clear();
			foreach (Guid contactId in order.ContactIds)
			{
				instance.ServiceOrderInfo.OrderContact.Add(contactId);
			}

			var dataHelperServiceOrderItem = new DataHelperServiceOrderItem(_connection);

			instance.ServiceOrderItems.Clear();
			if (order.OrderItems != null)
			{
				foreach (Models.ServiceOrderItems item in order.OrderItems)
				{
					item.ServiceOrderItem.ID = dataHelperServiceOrderItem.CreateOrUpdate(item.ServiceOrderItem);

					instance.ServiceOrderItems.Add(
						new ServiceOrderItemsSection
						{
							PriorityOrder = item.Priority,
							ServiceOrderItem = item.ServiceOrderItem.ID,
						});
				}
			}

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override List<Models.ServiceOrder> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new ServiceOrdersInstance(x))
				.ToList();

			var dataHelperServiceOrderItem = new DataHelperServiceOrderItem(_connection);
			var serviceOrderItems = dataHelperServiceOrderItem.Read();

			return instances.Select(
					x =>
					{
						List<Models.ServiceOrderItems> orderItems = x.ServiceOrderItems
							.Select(
								serviceOrderItemsSection => new Models.ServiceOrderItems
								{
									Priority = serviceOrderItemsSection.PriorityOrder,
									ServiceOrderItem = serviceOrderItems.Find(o => o.ID == serviceOrderItemsSection.ServiceOrderItem),
								})
							.Where(soi => soi.ServiceOrderItem != null)
							.ToList();

						return new Models.ServiceOrder
						{
							ID = x.ID.Id,
							StatusId = x.StatusId,
							OrderId = x.ServiceOrderInfo.ID,
							Name = x.ServiceOrderInfo.Name,
							Description = x.ServiceOrderInfo.Description,
							ExternalID = x.ServiceOrderInfo.ExternalID,
							Priority = x.ServiceOrderInfo.Priority,
							OrganizationId = x.ServiceOrderInfo.RelatedOrganization,
							ContactIds = x.ServiceOrderInfo.OrderContact ?? new List<Guid>(),
							OrderItems = orderItems,
						};
					})
				.ToList();
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.ServiceOrder item)
		{
			bool ok = true;

			var helper = new DataHelperServiceOrderItem(_connection);
			if (item.OrderItems != null)
			{
				foreach (var orderItem in item.OrderItems)
				{
					if (orderItem.ServiceOrderItem != null)
					{
						ok &= helper.TryDelete(orderItem.ServiceOrderItem);
					}
				}
			}

			return TryDelete(item.ID);
		}
	}
}