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
    Tel.    : +32 51 31 35 69
    Fax.    : +32 51 31 01 29
    E-mail    : info@skyline.be
    Web        : www.skyline.be
    Contact    : Ben Vandenberghe

****************************************************************************
Revision History:

DATE        VERSION        AUTHOR            COMMENTS

13/03/2025    1.0.0.1        RME, Skyline    Initial version
****************************************************************************
*/
namespace SLCSMCreateJobForServiceItem
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;

	using Library.Views;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.MediaOps.Common.IOData.Scheduling.Scripts.JobHandler;
	using Skyline.DataMiner.Utils.MediaOps.Helpers.Workflows;

	/// <summary>
	///     Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		/// <summary>
		///     The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			/*
            * Note:
            * Do not remove the commented methods below!
            * The lines are needed to execute an interactive automation script from the non-interactive automation script or from Visio!
            *
            * engine.ShowUI();
            */
			try
			{
				RunSafe(engine);
			}
			catch (ScriptAbortException)
			{
				// Catch normal abort exceptions (engine.ExitFail or engine.ExitSuccess)
			}
			catch (ScriptForceAbortException)
			{
				// Catch forced abort exceptions, caused via external maintenance messages.
			}
			catch (ScriptTimeoutException)
			{
				// Catch timeout exceptions for when a script has been running for too long.
			}
			catch (InteractiveUserDetachedException)
			{
				// Catch a user detaching from the interactive script by closing the window.
				// Only applicable for interactive scripts, can be removed for non-interactive scripts.
			}
			catch (Exception e)
			{
				var errorView = new ErrorView(engine, "Error", e.Message, e.ToString());
				new InteractiveController(engine).ShowDialog(errorView);
			}
		}

		private static string GetServiceItemName(DomInstance domInstance)
		{
			if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.Services.Id)
			{
				var instance = new ServicesInstance(domInstance);
				return instance.ServiceInfo.ServiceName;
			}

			if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceSpecifications.Id)
			{
				var instance = new ServiceSpecificationsInstance(domInstance);
				return instance.ServiceSpecificationInfo.SpecificationName;
			}

			return String.Empty;
		}

		private static ServiceItemsSection GetServiceItemSection(DomInstance domInstance, string label)
		{
			if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.Services.Id)
			{
				var instance = new ServicesInstance(domInstance);
				return instance.ServiceItems.FirstOrDefault(x => x.Label == label);
			}

			if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceSpecifications.Id)
			{
				var instance = new ServiceSpecificationsInstance(domInstance);
				return instance.ServiceItems.FirstOrDefault(x => x.Label == label);
			}

			throw new InvalidOperationException($"No Service item found with label '{label}'");
		}

		private static (DateTime?, DateTime?) GetServiceItemTimings(DomInstance domInstance)
		{
			if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.Services.Id)
			{
				var instance = new ServicesInstance(domInstance);
				return (instance.ServiceInfo.ServiceStartTime, instance.ServiceInfo.ServiceEndTime);
			}

			return (null, null);
		}

		private void RunSafe(IEngine engine)
		{
			string domIdRaw = engine.GetScriptParam("DOM ID").Value;
			Guid domId = JsonConvert.DeserializeObject<List<Guid>>(domIdRaw).FirstOrDefault();
			if (domId == Guid.Empty)
			{
				throw new InvalidOperationException("No DOM ID provided as input to the script");
			}

			var domHelper = new DomHelper(engine.SendSLNetMessages, SlcServicemanagementIds.ModuleId);
			var domInstance = domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(domId)).FirstOrDefault()
							  ?? throw new InvalidOperationException($"No DOM Instance with ID '{domId}' found on the system!");

			string label = engine.GetScriptParam("Service Item Label").Value.Trim('"', '[', ']');

			ServiceItemsSection serviceItemsSection = GetServiceItemSection(domInstance, label);

			var workflowHelper = new WorkflowHelper(engine);
			var workflow = workflowHelper.GetAllWorkflows().FirstOrDefault(x => x.Name == serviceItemsSection.DefinitionReference)
						   ?? throw new InvalidOperationException($"No Workflow found on the system with name '{serviceItemsSection.DefinitionReference}'");

			var timings = GetServiceItemTimings(domInstance);
			var jobConfiguration = new CreateJobAction
			{
				Name = $"{GetServiceItemName(domInstance)} | {serviceItemsSection.Label}",
				Description = $"{domInstance.ID.Id} | {serviceItemsSection.Label}",
				DomWorkflowId = workflow.Id,
				Source = "Scheduling",
				DesiredJobStatus = DesiredJobStatus.Tentative,
				Start = timings.Item1 ?? throw new InvalidOperationException("No Start Time configured to create the job from"),
				End = timings.Item2 ?? ReservationInstance.PermanentEnd,
			};

			OutputData sendToJobHandler = jobConfiguration.SendToJobHandler(engine, true);
			if (sendToJobHandler == null)
			{
				throw new InvalidOperationException("Failure on creating the job from the workflow");
			}

			if (sendToJobHandler.HasException)
			{
				engine.ExitFail(sendToJobHandler.ExceptionInfo.SourceException.Message);
				return;
			}

			var outputData = (CreateJobActionOutput)sendToJobHandler.ActionOutput;
			if (outputData == null)
			{
				throw new InvalidOperationException("Failure on creating the job from the workflow");
			}

			var jobId = outputData.DomJobId;

			var transitionJobToTentativeInputData = new ExecuteJobAction
			{
				DomJobId = jobId,
				JobAction = Skyline.DataMiner.Utils.MediaOps.Common.IOData.Scheduling.Scripts.JobHandler.JobAction.SaveAsTentative,
			};
			transitionJobToTentativeInputData.SendToJobHandler(engine, true);

			serviceItemsSection.ImplementationReference = jobId.ToString();
			AddOrUpdateServiceItemToInstance(domHelper, domInstance, serviceItemsSection, label);
		}

		private static void AddOrUpdateServiceItemToInstance(DomHelper helper, DomInstance domInstance, ServiceItemsSection newSection, string oldLabel)
		{
			if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.Services.Id)
			{
				var instance = new ServicesInstance(domInstance);

				// Remove old instance first in case of edit
				var oldItem = instance.ServiceItems.FirstOrDefault(x => x.Label == oldLabel);
				if (oldItem != null)
				{
					instance.ServiceItems.Remove(oldItem);
				}
				else
				{
					// Auto assign new ID
					long[] ids = instance.ServiceItems.Where(x => x.ServiceItemID.HasValue).Select(x => x.ServiceItemID.Value).OrderBy(x => x).ToArray();
					newSection.ServiceItemID = ids.Any() ? ids.Max() + 1 : 0;
				}

				instance.ServiceItems.Add(newSection);
				instance.Save(helper);
			}
			else if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceSpecifications.Id)
			{
				var instance = new ServiceSpecificationsInstance(domInstance);

				// Remove old instance first in case of edit
				var oldItem = instance.ServiceItems.FirstOrDefault(x => x.Label == oldLabel);
				if (oldItem != null)
				{
					instance.ServiceItems.Remove(oldItem);
				}
				else
				{
					// Auto assign new ID
					long[] ids = instance.ServiceItems.Where(x => x.ServiceItemID.HasValue).Select(x => x.ServiceItemID.Value).OrderBy(x => x).ToArray();
					newSection.ServiceItemID = ids.Any() ? ids.Max() + 1 : 0;
				}

				instance.ServiceItems.Add(newSection);
				instance.Save(helper);
			}
			else
			{
				throw new InvalidOperationException($"DOM definition '{domInstance.DomDefinitionId}' not supported (yet).");
			}
		}
	}
}
