namespace SLC_SM_GQIDS_Get_Service_Order_Items_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;

	using Library;

	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	using SLC_SM_Common.API.ServiceManagementApi;

	// Required to mark the interface as a GQI data source
	[GQIMetaData(Name = "Get_ServiceOrderItems")]
	public class EventManagerGetMultipleSections : IGQIDataSource, IGQIInputArguments, IGQIOnInit
	{
		// TO BE removed when we can easily fetch this using the DOM Code Generated code
		private static readonly List<ServiceOrderItemStatus> serviceOrderItemStatuseList = new List<ServiceOrderItemStatus>
		{
			new ServiceOrderItemStatus { Id = "06df5562-cd9b-4b0b-bd45-c58560a8b22a", Name = "New" },
			new ServiceOrderItemStatus { Id = "d917fc53-2638-4ab9-9ac6-651ec5312bac", Name = "Acknowledged" },
			new ServiceOrderItemStatus { Id = "331dc1c2-1950-4c00-a4ae-0aba674a30e6", Name = "In Progress" },
			new ServiceOrderItemStatus { Id = "260a7073-e54e-4482-a8a7-2b4f2e49c42e", Name = "Completed" },
			new ServiceOrderItemStatus { Id = "6a01a480-4c38-4db7-b545-72ba05742a7e", Name = "Rejected" },
			new ServiceOrderItemStatus { Id = "7f13d019-29de-43cb-a510-ab2b2a77e785", Name = "Failed" },
			new ServiceOrderItemStatus { Id = "f8a8d853-faaf-401c-9865-71e314614023", Name = "Partially Failed" },
			new ServiceOrderItemStatus { Id = "310ea9e9-f65c-4e11-8b1b-e2c34688ef44", Name = "Held" },
			new ServiceOrderItemStatus { Id = "23f9fa75-32b8-4e4a-bd65-06a7344d1902", Name = "Pending" },
			new ServiceOrderItemStatus { Id = "f7e93ddd-cddf-4755-a3e5-0f6ff885dcf5", Name = "Assess Cancellation" },
			new ServiceOrderItemStatus { Id = "15d08c01-fe63-4d5f-8544-e5b4d66439f5", Name = "Pending Cancellation" },
			new ServiceOrderItemStatus { Id = "61b80d48-d555-462e-baae-a52b17c85ddb", Name = "Cancelled" },
		};

		// defining input argument, will be converted to guid by OnArgumentsProcessed
		private readonly GQIStringArgument domIdArg = new GQIStringArgument("DOM ID") { IsRequired = false };
		private GQIDMS _dms;

		// variable where input argument will be stored
		private Guid _instanceDomId;

		public GQIColumn[] GetColumns()
		{
			return new GQIColumn[]
			{
				new GQIStringColumn("ID"),
				new GQIStringColumn("Name"),
				new GQIStringColumn("Start"),
				new GQIStringColumn("End"),
				new GQIStringColumn("Action"),
				new GQIStringColumn("Category"),
				new GQIStringColumn("Service Specification"),
				new GQIStringColumn("Service"),
				new GQIStringColumn("ServiceId"),
				new GQIStringColumn("Property"),
				new GQIStringColumn("Configuration"),
				new GQIStringColumn("Status"),
				new GQIStringColumn("StatusId"),
			};
		}

		public GQIArgument[] GetInputArguments()
		{
			return new GQIArgument[] { domIdArg };
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			return new GQIPage(GetMultiSection())
			{
				HasNextPage = false,
			};
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			// adds the input argument to private variable
			if (!Guid.TryParse(args.GetArgumentValue(domIdArg), out _instanceDomId))
			{
				_instanceDomId = Guid.Empty;
			}

			return new OnArgumentsProcessedOutputArgs();
		}

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			_dms = args.DMS;

			return default;
		}

		private static GQIRow BuildRow(Models.ServiceOrderItems item, Repo repo)
		{
			return new GQIRow(
				item.ServiceOrderItem.ID.ToString(),
				new[]
				{
					new GQICell { Value = item.ServiceOrderItem.ID.ToString() },
					new GQICell { Value = item.ServiceOrderItem.Name },
					new GQICell { Value = item.ServiceOrderItem.StartTime.HasValue ? item.ServiceOrderItem.StartTime.ToString() : "No Start Time" },
					new GQICell { Value = item.ServiceOrderItem.EndTime.HasValue ? item.ServiceOrderItem.EndTime.ToString() : "No End Time" },
					new GQICell { Value = item.ServiceOrderItem.Action },
					new GQICell
					{
						Value = item.ServiceOrderItem.ServiceCategoryId.HasValue
							? repo.ServiceCategories.Read().FirstOrDefault(x => x.ID == item.ServiceOrderItem.ServiceCategoryId)?.Name ?? String.Empty
							: String.Empty,
					},
					new GQICell
					{
						Value = item.ServiceOrderItem.SpecificationId.HasValue
							? repo.ServiceSpecifications.Read().FirstOrDefault(x => x.ID == item.ServiceOrderItem.SpecificationId)?.Name ?? String.Empty
							: String.Empty,
					},
					new GQICell
					{
						Value = item.ServiceOrderItem.ServiceId.HasValue
							? repo.Services.Read().FirstOrDefault(x => x.ID == item.ServiceOrderItem.ServiceId)?.Name ?? String.Empty
							: String.Empty,
					},
					new GQICell
					{
						Value = item.ServiceOrderItem.ServiceId?.ToString() ?? String.Empty,
					},
					new GQICell
					{
						Value = item.ServiceOrderItem.Properties != null
							? item.ServiceOrderItem.Properties.ID.ToString()
							: String.Empty,
					},
					new GQICell
					{
						Value = String.Empty, // Config has been replaced by multiple
					},
					new GQICell
					{
						Value = serviceOrderItemStatuseList.FirstOrDefault<ServiceOrderItemStatus>(status => status.Id == item.ServiceOrderItem.StatusId)?.Name.ToString() ?? "No status mapping",
					},
					new GQICell
					{
						Value = serviceOrderItemStatuseList.FirstOrDefault<ServiceOrderItemStatus>(status => status.Id == item.ServiceOrderItem.StatusId)?.Id.ToString() ?? "No status mapping",
					},
				});
		}

		private GQIRow[] GetMultiSection()
		{
			if (_instanceDomId == Guid.Empty)
			{
				// return th empty list
				return Array.Empty<GQIRow>();
			}

			var repo = new Repo(_dms.GetConnection());

			// create filter to filter event instances with specific dom event ids
			var instance = repo.ServiceOrders.Read().Find(x => x.ID == _instanceDomId);
			if (instance == null)
			{
				return Array.Empty<GQIRow>();
			}

			return instance.OrderItems.Where(x => x?.ServiceOrderItem != null).Select(item => BuildRow(item, repo)).ToArray();
		}

		public class ServiceOrderItemStatus
		{
			public string Id { get; set; } = String.Empty;

			public string Name { get; set; } = String.Empty;
		}
	}
}