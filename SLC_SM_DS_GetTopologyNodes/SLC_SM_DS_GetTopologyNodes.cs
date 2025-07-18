/*
****************************************************************************
*  Copyright (c) 2025,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

26/06/2025	1.0.0.1		RCA, Skyline	Initial version
****************************************************************************
*/

namespace SLCSMDSGetTopologyNodes
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcProperties;
	using DomHelpers.SlcServicemanagement;
	using DomHelpers.SlcWorkflow;
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <summary>
	/// Represents a data source.
	/// See: https://aka.dataminer.services/gqi-external-data-source for a complete example.
	/// </summary>
	[GQIMetaData(Name = "SLC_SM_DS_GetTopologyNodes")]
	public sealed class SLCSMDSGetTopologyNodes : IGQIDataSource
		, IGQIOnInit, IGQIInputArguments
	{
		private readonly GQIStringArgument domIdArg = new GQIStringArgument("DOM ID") { IsRequired = true };
		private Guid _domId;

		private GQIDMS _dms;

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			// Initialize the data source
			// See: https://aka.dataminer.services/igqioninit-oninit
			_dms = args.DMS;

			return default;
		}

		public GQIColumn[] GetColumns()
		{
			// Define data source columns
			// See: https://aka.dataminer.services/igqidatasource-getcolumns
			return new GQIColumn[]
			{
				new GQIStringColumn("ServiceItemId"),
				new GQIStringColumn("ServiceItemType"),
				new GQIStringColumn("ServiceItemLabel"),
				new GQIStringColumn("DefinitionReference"),
				new GQIStringColumn("ReferenceId"),
				new GQIStringColumn("Icon"),
			};
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			// Define data source rows
			// See: https://aka.dataminer.services/igqidatasource-getnextpage
			var smDomHelper = new DomHelper(_dms.SendMessages, SlcServicemanagementIds.ModuleId);

			var domInstance = smDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(_domId)).FirstOrDefault();
			if (domInstance == null)
				throw new InvalidOperationException($"Could not find DOM instance with id {_domId}");

			var serviceItems = ServiceInstancesExtentions.GetTypedInstance(domInstance)
				.GetServiceItems()
				.Where(i => !i.IsEmpty && !string.IsNullOrEmpty(i.Label));

			var workflowPropertyValues = new DomHelper(_dms.SendMessages, SlcPropertiesIds.ModuleId)
				.DomInstances
				.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcPropertiesIds.Definitions.PropertyValues.Id))
				.Select(p => new PropertyValuesInstance(p));

			var workflowsFilter = serviceItems
				.Where(item => item.ServiceItemType.Value == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Workflow)
				.Select(item => DomInstanceExposers.Name.Equal(item.DefinitionReference))
				.Aggregate<FilterElement<DomInstance>, FilterElement<DomInstance>>(null, (acc, next) => acc == null ? next : acc.OR(next));

			var workflows = workflowsFilter != null
				? new DomHelper(_dms.SendMessages, SlcWorkflowIds.ModuleId)
					.DomInstances
					.Read(workflowsFilter)
				: Enumerable.Empty<DomInstance>();

			var messages = serviceItems
				.Where(item => item.ServiceItemType.Value == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.SRMBooking)
				.Select(item => new GetLiteElementInfo { NameFilter = item.DefinitionReference })
				.ToArray();

			var elements = _dms.SendMessages(messages)
				.OfType<LiteElementInfoEvent>();

			var getPropertyMessages = elements
				.Select(element => new GetPropertyValueMessage
				{
					ObjectID = $"{element.DataMinerID}/{element.ElementID}",
					ObjectType = "Element",
					PropertyName = "Icon",
				})
				.Cast<DMSMessage>()
				.ToArray();

			var elementIcons = _dms.SendMessages(getPropertyMessages)
				.OfType<PropertyChangeEventMessage>();

			var rows = serviceItems
				.Select(i => BuildRow(i, GetReferenceId(i, elements, workflows), GetIcon(i, elements, workflows, workflowPropertyValues, elementIcons)))
				.ToArray();

			return new GQIPage(rows);
		}

		public GQIArgument[] GetInputArguments()
		{
			return new GQIArgument[]
			{
				domIdArg,
			};
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			if (!Guid.TryParse(args.GetArgumentValue(domIdArg), out _domId))
				_domId = Guid.Empty;

			return new OnArgumentsProcessedOutputArgs();
		}

		private string GetReferenceId(ServiceItemsSection item, IEnumerable<LiteElementInfoEvent> elements, IEnumerable<DomInstance> workflows)
		{
			if (item.ServiceItemType == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.SRMBooking)
			{
				var element = elements.FirstOrDefault(e => e.Name == item.DefinitionReference);
				return $"{element.DataMinerID}/{element.ElementID}";
			}

			if (item.ServiceItemType == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Workflow)
			{
				var workflow = workflows.FirstOrDefault(i => i.Name == item.DefinitionReference);
				return workflow.ID.Id.ToString();
			}

			throw new InvalidOperationException("Unsupported service item type");
		}

		private string GetIcon(
			ServiceItemsSection item,
			IEnumerable<LiteElementInfoEvent> elements,
			IEnumerable<DomInstance> workflows,
			IEnumerable<PropertyValuesInstance> propertyValues,
			IEnumerable<PropertyChangeEventMessage> icons)
		{
			if (item.ServiceItemType == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.SRMBooking)
			{
				var element = elements.FirstOrDefault(e => e.Name == item.DefinitionReference);
				return icons
					.FirstOrDefault(i => i.DataMinerID == element.DataMinerID && i.ElementID == element.ElementID)?
					.Value ?? string.Empty;
			}

			if (item.ServiceItemType == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Workflow)
			{
				var workflow = workflows.FirstOrDefault(i => i.Name == item.DefinitionReference);
				return propertyValues
					.FirstOrDefault(p => p.PropertyValueInfo.LinkedObjectID == workflow.ID.Id.ToString())?
					.PropertyValue.FirstOrDefault(v => v.PropertyName == "Icon")?.Value ?? string.Empty;
			}

			throw new InvalidOperationException("Unsupported service item type");
		}

		private GQIRow BuildRow(ServiceItemsSection serviceItem, string referenceId, string icon)
		{
			return new GQIRow(
				new[]
				{
					new GQICell { Value = serviceItem.ServiceItemID.ToString() },
					new GQICell { Value = serviceItem.ServiceItemType.ToString() },
					new GQICell { Value = serviceItem.Label },
					new GQICell { Value = serviceItem.DefinitionReference },
					new GQICell { Value = referenceId },
					new GQICell { Value = icon },
				});
		}
	}
}
