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

27/05/2025	1.0.0.1		RCA, Skyline	Initial version
****************************************************************************
*/

namespace GetServiceItemRelationshipMultisection
{
	using System;
	using System.Linq;

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
	[GQIMetaData(Name = "Get Service Item Relationship Multisection")]
	public sealed class GetServiceItemRelationshipMultisection : IGQIDataSource, IGQIOnInit, IGQIInputArguments
	{
		private readonly GQIStringArgument domIdArg = new GQIStringArgument("DOM ID") { IsRequired = false };
		private Guid _specificationId;

		private IServiceInstanceBase _serviceInstance;

		private DomHelper _smDomHelper;
		private DomHelper _wfDomHelper;
		private GQIDMS dms;

		private WorkflowsInstance[] _workflows;

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			dms = args.DMS;

			return default;
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
			if (!Guid.TryParse(args.GetArgumentValue(domIdArg), out _specificationId))
				_specificationId = Guid.Empty;

			return new OnArgumentsProcessedOutputArgs();
		}

		public GQIColumn[] GetColumns()
		{
			return new GQIColumn[]
			{
				new GQIStringColumn("ID"),
				new GQIStringColumn("Type"),
				new GQIStringColumn("Child"),
				new GQIStringColumn("Parent"),
				new GQIStringColumn("Child Interface"),
				new GQIStringColumn("Parent Interface"),
				new GQIStringColumn("Source"),
				new GQIStringColumn("Destination"),
				new GQIStringColumn("Source Interface"),
				new GQIStringColumn("Destination Interface"),
			};
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			if (_specificationId == Guid.Empty)
				return EmptyPage();

			Init();

			var relationships = _serviceInstance.GetServiceItemRelationships();
			var items = _serviceInstance.GetServiceItems().Select(i => i.ServiceItemID.ToString()).ToArray();

			return new GQIPage(relationships
				.Where(r => !r.IsEmpty
					&& items.Contains(r.ChildServiceItem)
					&& items.Contains(r.ParentServiceItem))
				.Select(BuildRow)
				.ToArray());
		}

		private void Init()
		{
			_smDomHelper = new DomHelper(dms.SendMessages, SlcServicemanagementIds.ModuleId);
			_wfDomHelper = new DomHelper(dms.SendMessages, SlcWorkflowIds.ModuleId);

			_workflows = _wfDomHelper.DomInstances
				.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcWorkflowIds.Definitions.Workflows.Id))
				.Select(w => new WorkflowsInstance(w))
				.ToArray();

			var domInstance = _smDomHelper.DomInstances
				.Read(DomInstanceExposers.Id.Equal(_specificationId))
				.FirstOrDefault();

			_serviceInstance = ServiceInstancesExtentions.GetTypedInstance(domInstance);
		}

		private string GetReferencedObjectName(string serviceItemId)
		{
			return _serviceInstance.GetServiceItems()
			.FirstOrDefault(i => i.ServiceItemID.ToString() == serviceItemId)?.DefinitionReference ?? string.Empty;
		}

		private GQIRow BuildRow(ServiceItemRelationshipSection r)
		{
			return new GQIRow(new[]
			{
				new GQICell { Value = r.SectionID.Id.ToString() },
				new GQICell { Value = r.Type },
				new GQICell { Value = r.ChildServiceItem },
				new GQICell { Value = r.ParentServiceItem },
				new GQICell { Value = r.ChildServiceItemInterfaceID },
				new GQICell { Value = r.ParentServiceItemInterfaceID },
				new GQICell { Value = GetReferencedObjectName(r.ParentServiceItem) },
				new GQICell { Value = GetReferencedObjectName(r.ChildServiceItem) },
				new GQICell { Value = GetInterfaceName(r.ParentServiceItem, r.ParentServiceItemInterfaceID) },
				new GQICell { Value = GetInterfaceName(r.ChildServiceItem, r.ChildServiceItemInterfaceID) },
			});
		}

		private string GetInterfaceName(string serviceItemId, string interfaceId)
		{
			var serviceItem = _serviceInstance.GetServiceItems()
				                  .FirstOrDefault(item => item.ServiceItemID.ToString() == serviceItemId)
				?? throw new InvalidOperationException($"No Service Item found on the system with ID '{serviceItemId}'");

			var type = serviceItem.ServiceItemType.Value;

			if (type == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Workflow)
				return GetWorkflowInterfaceName(serviceItemId, interfaceId);

			if (type == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.SRMBooking)
				return interfaceId == "1" ? "Default SRM Output" : "Default SRM Input";

			if (type == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Service)
				return interfaceId == "1" ? "Default Service Link Output" : "Default Service Link Input";

			throw new InvalidOperationException($"Unrecognized service item type {type}");
		}

		private string GetWorkflowInterfaceName(string serviceItemId, string interfaceId)
		{
			var workflowName = GetReferencedObjectName(serviceItemId);
			var node = _workflows
				.FirstOrDefault(w => w.Name == workflowName)?.Nodes
				.FirstOrDefault(n => n.NodeID == interfaceId);

			return node?.NodeAlias ?? string.Empty;
		}

		private GQIPage EmptyPage()
		{
			return new GQIPage(Array.Empty<GQIRow>());
		}
	}
}
