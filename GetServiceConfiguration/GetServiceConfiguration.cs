namespace Get_ServiceConfiguration_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcConfigurations;
	using DomHelpers.SlcServicemanagement;

	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	using Guid = System.Guid;

	[GQIMetaData(Name = "Get_ServiceConfiguration")]
	public class EventManagerGetMultipleSections : IGQIDataSource, IGQIInputArguments, IGQIOnInit
	{
		// defining input argument, will be converted to guid by OnArgumentsProcessed
		private readonly GQIStringArgument domIdArg = new GQIStringArgument("DOM ID") { IsRequired = true };
		private DomHelper _domHelperSrvMgmt;
		private DomHelper _domHelperConfig;
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
				new GQIBooleanColumn("Mandatory At Service Level"),
				new GQIBooleanColumn("Expose At Service Order Level"),
				new GQIBooleanColumn("Mandatory At Service Order Level"),
				new GQIStringColumn("Type"),
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

		private static DomInstance FetchDomInstance(DomHelper helper, Guid instanceDomId)
		{
			return helper.DomInstances.Read(DomInstanceExposers.Id.Equal(instanceDomId)).FirstOrDefault();
		}

		private static List<DomInstance> FetchDomInstances(DomHelper helper, List<Guid> instanceDomIds)
		{
			// create filter to filter event instances with specific dom event ids
			FilterElement<DomInstance> filter = new ORFilterElement<DomInstance>();
			foreach (Guid guid in instanceDomIds)
			{
				filter = filter.OR(DomInstanceExposers.Id.Equal(guid));
			}

			return helper.DomInstances.Read(filter);
		}

		private GQIRow[] GetMultiSection()
		{
			if (instanceDomId == Guid.Empty)
			{
				return Array.Empty<GQIRow>();
			}

			// will initiate DomHelper
			LoadApplicationHandlersAndHelpers();

			var domInstance = FetchDomInstance(_domHelperSrvMgmt, instanceDomId);
			if (domInstance == null)
			{
				return Array.Empty<GQIRow>();
			}

			IList<Guid> serviceConfigurationGuids = new List<Guid>();

			if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.Services.Id)
			{
				var instance = new ServicesInstance(domInstance);
				serviceConfigurationGuids = instance.ServiceInfo.ServiceConfigurationParameters;
			}
			else if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceSpecifications.Id)
			{
				var instance = new ServiceSpecificationsInstance(domInstance);
				serviceConfigurationGuids = instance.ServiceSpecificationInfo.ServiceSpecificationConfigurationParameters;
			}
			else if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceOrderItems.Id)
			{
				var instance = new ServiceOrderItemsInstance(domInstance);
				serviceConfigurationGuids = instance.ServiceOrderItemServiceInfo.ServiceOrderItemConfigurations;
			}
			else
			{
				// For future options
			}

			if (serviceConfigurationGuids == null || !serviceConfigurationGuids.Any())
			{
				return Array.Empty<GQIRow>();
			}

			var configDomInstances = FetchDomInstances(_domHelperSrvMgmt, serviceConfigurationGuids.ToList());
			if (configDomInstances == null)
			{
				return Array.Empty<GQIRow>();
			}

			var rows = new List<GQIRow>();

			foreach (DomInstance instance in configDomInstances)
			{
				var configValueInstance = new ServiceSpecificationConfigurationValueInstance(instance);

				var configValue = configValueInstance.ServiceSpecificationConfigurationValue.ConfigurationParameterValue;
				if (configValue == default)
				{
					continue;
				}

				var item = new ConfigurationParameterValueInstance(FetchDomInstance(_domHelperConfig, configValue.Value));

				rows.Add(
				new GQIRow(
					new[]
					{
						new GQICell { Value = item.ConfigurationParameterValue.Label ?? String.Empty },
						new GQICell { Value = configValueInstance.ServiceSpecificationConfigurationValue.MandatoryAtServiceLevel ?? false },
						new GQICell { Value = configValueInstance.ServiceSpecificationConfigurationValue.MandatoryAtServiceOrderLevel ?? false },
						new GQICell { Value = configValueInstance.ServiceSpecificationConfigurationValue.ExposeAtServiceOrderLevel ?? false },
						new GQICell { Value = item.ConfigurationParameterValue.Type.HasValue ? item.ConfigurationParameterValue.Type.Value.ToString() : SlcConfigurationsIds.Enums.Type.Text.ToString() },
						new GQICell
						{
							Value = !String.IsNullOrWhiteSpace(item.ConfigurationParameterValue.StringValue)
								? item.ConfigurationParameterValue.StringValue
								: item.ConfigurationParameterValue.DoubleValue.HasValue
									? Convert.ToString(item.ConfigurationParameterValue.DoubleValue.Value)
									: String.Empty,
						},
					}));
			}

			return rows.ToArray();
		}

		private void LoadApplicationHandlersAndHelpers()
		{
			_domHelperSrvMgmt = new DomHelper(dms.SendMessages, SlcServicemanagementIds.ModuleId);
			_domHelperConfig = new DomHelper(dms.SendMessages, SlcConfigurationsIds.ModuleId);
		}
	}
}