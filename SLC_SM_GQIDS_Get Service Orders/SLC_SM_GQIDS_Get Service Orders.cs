namespace SLC_SM_GQIDS_Get_Service_Orders
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;

	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;

	using Models = Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement.Models;

	// Required to mark the interface as a GQI data source
	[GQIMetaData(Name = "Get_ServiceOrders")]
	public class GetServiceOrders : IGQIDataSource, IGQIInputArguments, IGQIOnInit
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
		private GQIDMS _dms;

		public DMSMessage GenerateInformationEvent(string message)
		{
			var generateAlarmMessage = new GenerateAlarmMessage(GenerateAlarmMessage.AlarmSeverity.Information, message) { Status = GenerateAlarmMessage.AlarmStatus.Cleared };
			return _dms.SendMessage(generateAlarmMessage);
		}

		public GQIColumn[] GetColumns()
		{
			return new GQIColumn[]
			{
				new GQIStringColumn("Order ID"),
				new GQIStringColumn("Name"),
				new GQIStringColumn("Description"),
				new GQIStringColumn("Priority"),
				new GQIStringColumn("External ID"),
				new GQIStringColumn("Related Organization"),
				new GQIStringColumn("State"),
			};
		}

		public GQIArgument[] GetInputArguments()
		{
			return Array.Empty<GQIArgument>();
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			try
			{
				return new GQIPage(GetMultiSection())
				{
					HasNextPage = false,
				};
			}
			catch (Exception e)
			{
				GenerateInformationEvent(e.ToString());
				return new GQIPage(Array.Empty<GQIRow>());
			}
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			return new OnArgumentsProcessedOutputArgs();
		}

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			_dms = args.DMS;

			return default;
		}

		private GQIRow BuildRow(Models.ServiceOrder item, List<Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization.Models.Organization> organizations)
		{
			return new GQIRow(
				item.ID.ToString(),
				new[]
				{
					new GQICell { Value = item.ID.ToString() },
					new GQICell { Value = item.Name ?? String.Empty },
					new GQICell { Value = item.Description ?? String.Empty },
					new GQICell { Value = item.Priority?.ToString() ?? "Low" },
					new GQICell { Value = item.ExternalID ?? String.Empty },
					new GQICell { Value = item.OrganizationId.HasValue ? organizations.Find(x => x.ID == item.OrganizationId)?.Name ?? String.Empty : String.Empty },
					new GQICell { Value = statusNames.ContainsKey(item.StatusId) ? statusNames[item.StatusId] : "No status mapping" },
				});
		}

		private GQIRow[] GetMultiSection()
		{
			IConnection connection = _dms.GetConnection();
			List<Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization.Models.Organization> organizations = new DataHelperOrganization(connection).Read();

			var instances = new DataHelperServiceOrder(connection).Read();
			return instances.Select(item => BuildRow(item, organizations)).ToArray();
		}
	}
}