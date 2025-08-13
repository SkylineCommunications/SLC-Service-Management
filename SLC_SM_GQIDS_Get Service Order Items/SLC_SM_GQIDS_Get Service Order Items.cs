namespace SLC_SM_GQIDS_Get_Service_Order_Items_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;

	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;

	// Required to mark the interface as a GQI data source
	[GQIMetaData(Name = "Get_ServiceOrderItems")]
	public class EventManagerGetMultipleSections : IGQIDataSource, IGQIInputArguments, IGQIOnInit
	{
		// TO BE removed when we can easily fetch this using the DOM Code Generated code
		private static readonly Dictionary<string, string> statusNames = new Dictionary<string, string>
		{
			{ SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Statuses.New, "New" },
			{ SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Statuses.Acknowledged, "Acknowledged" },
			{ SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Statuses.InProgress, "In Progress" },
			{ SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Statuses.Completed, "Completed" },
			{ SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Statuses.Rejected, "Rejected" },
			{ SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Statuses.Failed, "Failed" },
			{ SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Statuses.PartiallyFailed, "Partially Failed" },
			{ SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Statuses.Held, "Held" },
			{ SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Statuses.Pending, "Pending" },
			{ SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Statuses.AssessCancellation, "Assess Cancellation" },
			{ SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Statuses.PendingCancellation, "Pending Cancellation" },
			{ SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Statuses.Cancelled, "Cancelled" },
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

		private static GQIRow BuildRow(Models.ServiceOrderItems item, List<Models.ServiceCategory> categories, List<Models.ServiceSpecification> specifications, List<Models.Service> services)
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
							? categories.FirstOrDefault(x => x.ID == item.ServiceOrderItem.ServiceCategoryId)?.Name ?? String.Empty
							: String.Empty,
					},
					new GQICell
					{
						Value = item.ServiceOrderItem.SpecificationId.HasValue
							? specifications.FirstOrDefault(x => x.ID == item.ServiceOrderItem.SpecificationId)?.Name ?? String.Empty
							: String.Empty,
					},
					new GQICell
					{
						Value = item.ServiceOrderItem.ServiceId.HasValue
							? services.FirstOrDefault(x => x.ID == item.ServiceOrderItem.ServiceId)?.Name ?? String.Empty
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
						Value = statusNames.ContainsKey(item.ServiceOrderItem.StatusId) ? statusNames[item.ServiceOrderItem.StatusId] : "No status mapping",
					},
					new GQICell
					{
						Value = item.ServiceOrderItem.StatusId ?? "No status mapping",
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

			IConnection connection = _dms.GetConnection();
			var orders = new DataHelperServiceOrder(connection).Read();
			var categories = new DataHelperServiceCategory(connection).Read();
			var specifications = new DataHelperServiceSpecification(connection).Read();
			var services = new DataHelperService(connection).Read();

			// create filter to filter event instances with specific dom event ids
			var instance = orders.Find(x => x.ID == _instanceDomId);
			if (instance == null)
			{
				return Array.Empty<GQIRow>();
			}

			return instance.OrderItems.Where(x => x?.ServiceOrderItem != null).Select(item => BuildRow(item, categories, specifications, services)).ToArray();
		}
	}
}