namespace Get_ServiceItemsMultipleSections_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.Messages;

	// Required to mark the interface as a GQI data source
	[GQIMetaData(Name = "Get_ServiceItemsMultipleSections")]
	public class EventManagerGetMultipleSections : IGQIDataSource, IGQIInputArguments, IGQIOnInit
	{
		// defining input argument, will be converted to guid by OnArgumentsProcessed
		private readonly GQIStringArgument domIdArg = new GQIStringArgument("DOM ID") { IsRequired = true };
		private DomHelper _domHelper;
		private DomInstance _domInstance;
		private GQIDMS dms;

		// variable where input argument will be stored
		private Guid instanceDomId;

		public DMSMessage GenerateInformationEvent(string message)
		{
			var generateAlarmMessage = new GenerateAlarmMessage(GenerateAlarmMessage.AlarmSeverity.Information, message);
			return dms.SendMessage(generateAlarmMessage);
		}

		public GQIColumn[] GetColumns()
		{
			////if (sectionName == "Feeds")
			////{
			////    return new GQIColumn[]
			////            {
			////            new GQIBooleanColumn("Selected"),
			////            new GQIStringColumn("Feed Role"),
			////            new GQIStringColumn("Feed Reference"),
			////            };
			////}
			////else
			////{
			////    return new GQIColumn[]
			////    {
			////        new GQIStringColumn("Not"),
			////        new GQIStringColumn("Supported"),
			////    };
			////}

			return new GQIColumn[]
			{
				new GQIStringColumn("Label"),
				new GQIStringColumn("Service parameter ID"),
				new GQIStringColumn("Profile parameter ID"),
				new GQIBooleanColumn("Mandatory"),
				new GQIStringColumn("Value"),
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

		private DomInstance FetchDomInstance(Guid instanceDomId)
		{
			var domIntanceId = new DomInstanceId(instanceDomId);

			// create filter to filter event instances with specific dom event ids
			var filter = DomInstanceExposers.Id.Equal(domIntanceId);

			return _domHelper.DomInstances.Read(filter).FirstOrDefault();
		}

		private GQIRow[] GetMultiSection()
		{
			////GenerateInformationEvent("Get Service Items Multisection started");

			// define output list
			var rows = new List<GQIRow>();

			if (instanceDomId == Guid.Empty)
			{
				// return th empty list
				return rows.ToArray();
			}

			// will initiate DomHelper
			LoadApplicationHandlersAndHelpers();

			_domInstance = FetchDomInstance(instanceDomId);
			Guid serviceConfigurationGuid;

			if (_domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.Services.Id)
			{
				var instance = new ServicesInstance(_domInstance);

				serviceConfigurationGuid = instance.ServiceInfo.ServiceConfiguration.Value;
			}
			else if (_domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceSpecifications.Id)
			{
				var instance = new ServiceSpecificationsInstance(_domInstance);
				serviceConfigurationGuid = instance.ServiceSpecificationInfo.ServiceConfiguration.Value;
			}
			else
			{
				return rows.ToArray();
			}

			var configDomInstance = FetchDomInstance(serviceConfigurationGuid);

			var configIntance = new ServiceConfigurationInstance(configDomInstance);

			var configValues = configIntance.ServiceConfigurationParametersValues;

			// GenerateInformationEvent("test");
			configValues.ForEach(
				item =>
				{
					rows.Add(
						new GQIRow(
							new[]
							{
								new GQICell { Value = item.Label },
								new GQICell { Value = item.ServiceParameterID },
								new GQICell { Value = item.ProfileParameterID },
								new GQICell { Value = item.Mandatory },
								new GQICell { Value = item.StringValue ?? item.DoubleValue.ToString() },
							}));
				});

			return rows.ToArray();
		}

		private void LoadApplicationHandlersAndHelpers()
		{
			_domHelper = new DomHelper(dms.SendMessages, SlcServicemanagementIds.ModuleId);
		}
	}
}