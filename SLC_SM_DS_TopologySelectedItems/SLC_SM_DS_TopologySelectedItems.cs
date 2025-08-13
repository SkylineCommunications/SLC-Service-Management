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

11/06/2025	1.0.0.1		RCA, Skyline	Initial version
****************************************************************************
*/

namespace SLCSMDSTopologySelectedItems
{
	using System.Linq;
	using Skyline.DataMiner.Analytics.GenericInterface;

	/// <summary>
	/// Represents a data source.
	/// See: https://aka.dataminer.services/gqi-external-data-source for a complete example.
	/// </summary>
	[GQIMetaData(Name = "SLC_SM_DS_TopologySelectedItems")]
	public sealed class SLCSMDSTopologySelectedItems : IGQIDataSource, IGQIInputArguments
	{
		private readonly GQIStringArgument nodeIdsArg = new GQIStringArgument("NodeIds") { IsRequired = false };
		private string _nodeIds;

		private readonly GQIStringArgument connectionIdsArg = new GQIStringArgument("ConnectionIds") { IsRequired = false };
		private string _connectionIds;

		public GQIArgument[] GetInputArguments()
		{
			return new GQIArgument[] { nodeIdsArg, connectionIdsArg };
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			_nodeIds = args.GetArgumentValue(nodeIdsArg);
			_connectionIds = args.GetArgumentValue(connectionIdsArg);

			return new OnArgumentsProcessedOutputArgs();
		}

		public GQIColumn[] GetColumns()
		{
			return new GQIColumn[]
			{
				new GQIStringColumn("Item"),
				new GQIStringColumn("Ids"),
			};
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			string ids, type;

			if ((_nodeIds == null || !_nodeIds.Any())
				&& (_connectionIds == null || !_connectionIds.Any()))
			{
				type = "None";
				ids = string.Empty;
			}
			else if (_nodeIds != null && _nodeIds.Any())
			{
				type = "Node";
				ids = _nodeIds;
			}
			else
			{
				type = "Connection";
				ids = _connectionIds;
			}

			return new GQIPage(new[] { BuildRow(type, ids) });
		}

		private GQIRow BuildRow(string type, string ids)
		{
			return new GQIRow(
			new[]
			{
					new GQICell { Value = type },
					new GQICell { Value = ids },
			});
		}
	}
}
