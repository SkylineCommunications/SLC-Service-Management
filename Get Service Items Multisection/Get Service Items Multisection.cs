namespace Get_ServiceItemsMultipleSections_1
{
	// Used to process the Service Items
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using SLDataGateway.API.Querying;

	// Required to mark the interface as a GQI data source
	[GQIMetaData(Name = "Get_ServiceItemsMultipleSections")]
	public class EventManagerGetMultipleSections : IGQIDataSource, IGQIInputArguments, IGQIOnInit
	{
		// defining input argument, will be converted to guid by OnArgumentsProcessed
		private readonly GQIStringArgument domIdArg = new GQIStringArgument("DOM ID") { IsRequired = true };
		private DomHelper _domHelper;
		private GQIDMS dms;

		// variable where input argument will be stored
		private Guid instanceDomId;

		public DMSMessage GenerateInformationEvent(string message)
		{
			var generateAlarmMessage = new GenerateAlarmMessage(GenerateAlarmMessage.AlarmSeverity.Information, message) { Status = GenerateAlarmMessage.AlarmStatus.Cleared };
			return dms.SendMessage(generateAlarmMessage);
		}

		public GQIColumn[] GetColumns()
		{
			return new GQIColumn[]
			{
				new GQIStringColumn("Label"),
				new GQIIntColumn("Service Item ID"),
				new GQIStringColumn("Service Item Type"),
				new GQIStringColumn("Definition Reference"),
				new GQIStringColumn("Service Item Script"),
				new GQIStringColumn("Implementation Reference"),
				new GQIStringColumn("Implementation Reference Name"),
			};
		}

		public GQIArgument[] GetInputArguments()
		{
			return new GQIArgument[]
			{
				domIdArg,
			};
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			////GenerateInformationEvent("GetNextPage started");
			return new GQIPage(GetMultiSection())
			{
				HasNextPage = false,
			};
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			// adds the input argument to private variable
			if (!Guid.TryParse(args.GetArgumentValue(domIdArg), out instanceDomId))
			{
				instanceDomId = Guid.Empty;
			}

			return new OnArgumentsProcessedOutputArgs();
		}

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			dms = args.DMS;

			return default;
		}

		private GQIRow[] GetMultiSection()
		{
			////GenerateInformationEvent("Get Service Items Multisection started");
			if (instanceDomId == Guid.Empty)
			{
				// return th empty list
				return Array.Empty<GQIRow>();
			}

			// will initiate DomHelper
			LoadApplicationHandlersAndHelpers();

			var domIntanceId = new DomInstanceId(instanceDomId);

			// create filter to filter event instances with specific dom event ids
			var filter = DomInstanceExposers.Id.Equal(domIntanceId);

			var domInstance = _domHelper.DomInstances.Read(filter).FirstOrDefault();
			if (domInstance == null)
			{
				return Array.Empty<GQIRow>();
			}

			// Service item list to fill with either service or service specifiation's service items
			IList<ServiceItemsSection> serviceItems = new List<ServiceItemsSection>();

			if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.Services.Id)
			{
				var instance = new ServicesInstance(domInstance);

				serviceItems = instance.ServiceItems;
			}
			else if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceSpecifications.Id)
			{
				var instance = new ServiceSpecificationsInstance(domInstance);
				serviceItems = instance.ServiceItems;
			}
			else
			{
				// For future use
			}

			// GenerateInformationEvent("test");
			return serviceItems
				.Where(x => !String.IsNullOrEmpty(x.Label))
				.Select(BuildRow)
				.ToArray();
		}

		private GQIRow BuildRow(ServiceItemsSection item)
		{
			return new GQIRow(
				new[]
				{
					new GQICell { Value = item.Label },
					new GQICell { Value = (int)(item.ServiceItemID ?? 0) },
					new GQICell { Value = item.ServiceItemType.HasValue ? SlcServicemanagementIds.Enums.Serviceitemtypes.ToValue(item.ServiceItemType.Value) : String.Empty },
					new GQICell { Value = item.DefinitionReference ?? String.Empty },
					new GQICell { Value = item.ServiceItemScript ?? String.Empty },
					new GQICell { Value = item.ImplementationReference ?? String.Empty },
					new GQICell { Value = GetImplementationRefName(item.ImplementationReference) },
				});
		}

		private string GetImplementationRefName(string reference)
		{
			if (String.IsNullOrEmpty(reference) || !Guid.TryParse(reference, out Guid id))
			{
				return String.Empty;
			}

			var inst = new DomHelper(dms.SendMessages, "(slc)workflow").DomInstances.Read(DomInstanceExposers.Id.Equal(id)).FirstOrDefault();
			if (inst != null)
			{
				return inst.Name;
			}

			var request = new ManagerStoreStartPagingRequest<ReservationInstance>(ReservationInstanceExposers.ID.Equal(id).ToQuery(), 1000);
			var response = (ManagerStorePagingResponse<ReservationInstance>)dms.SendMessage(request);

			return response?.Objects.FirstOrDefault()?.Name ?? String.Empty;
		}

		private void LoadApplicationHandlersAndHelpers()
		{
			_domHelper = new DomHelper(dms.SendMessages, SlcServicemanagementIds.ModuleId);
		}
	}
}
