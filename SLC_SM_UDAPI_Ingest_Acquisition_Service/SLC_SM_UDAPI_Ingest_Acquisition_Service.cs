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

13/06/2025	1.0.0.1		RME, Skyline	Initial version
****************************************************************************
*/

namespace SLC_SM_UDAPI_Ingest_Acquisition_Service
{
	using System;

	using ACQ_LIB;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis;
	using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;

	/// <summary>
	/// Represents a DataMiner user-defined API.
	/// </summary>
	public class Script
	{
		/// <summary>
		/// The API trigger.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		/// <param name="requestData">Holds the API request data.</param>
		/// <returns>An object with the script API output data.</returns>
		[AutomationEntryPoint(AutomationEntryPointType.Types.OnApiTrigger)]
		public ApiTriggerOutput OnApiTrigger(IEngine engine, ApiTriggerInput requestData)
		{
			try
			{
				var method = requestData.RequestMethod;
				if (method != RequestMethod.Post && method != RequestMethod.Put)
				{
					return new ApiTriggerOutput
					{
						ResponseCode = (int)StatusCode.BadRequest,
						ResponseBody = "Only method POST is supported.",
					};
				}

				var acquisitionData = ParseInput(requestData.RawBody);
				if (acquisitionData == null)
				{
					return new ApiTriggerOutput
					{
						ResponseCode = (int)StatusCode.BadRequest,
						ResponseBody = "The provided content could not be parsed to a valid Acquisition Data structure.",
					};
				}

				var script = engine.PrepareSubScript("SLC_SM_AS_Ingest_Acquisition_Data");
				script.Synchronous = true;
				script.SelectScriptParam("Data", requestData.RawBody);

				script.StartScript();

				if (script.HadError)
				{
					return new ApiTriggerOutput
					{
						ResponseCode = (int)StatusCode.InternalServerError,
						ResponseBody = $"Failed to add or update the acquisition data '{acquisitionData.Name} ({acquisitionData.Id})' due to: {String.Join(Environment.NewLine, script.GetErrorMessages())}",
					};
				}
				else
				{
					return new ApiTriggerOutput
					{
						ResponseCode = (int)StatusCode.Ok,
						ResponseBody = $"Acquisition Data '{acquisitionData.Name} ({acquisitionData.Id})' added or updated",
					};
				}
			}
			catch (Exception e)
			{
				return new ApiTriggerOutput
				{
					ResponseCode = (int)StatusCode.InternalServerError,
					ResponseBody = e.ToString(),
				};
			}
		}

		private LupoAcquisitionData.Root ParseInput(string rawBody)
		{
			try
			{
				return JsonConvert.DeserializeObject<LupoAcquisitionData.Root>(rawBody);
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}