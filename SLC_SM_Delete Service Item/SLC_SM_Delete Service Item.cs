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

dd/mm/2025    1.0.0.1        XXX, Skyline    Initial version
****************************************************************************
*/
namespace SLC_SM_Delete_Service_Item_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;

	using Library.Views;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.Relationship;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.MediaOps.Common.IOData.Scheduling.Scripts.JobHandler;
	using Skyline.DataMiner.Utils.MediaOps.Helpers.Scheduling;

	using Models = Skyline.DataMiner.ProjectApi.ServiceManagement.API.Relationship.Models;

	/// <summary>
	///     Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		private InteractiveController _controller;
		private IEngine _engine;

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
				_engine = engine;
				_controller = new InteractiveController(engine) { ScriptAbortPopupBehavior = ScriptAbortPopupBehavior.HideAlways };
				RunSafe();
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
				_controller.ShowDialog(errorView);
			}
		}

		private void DeleteServiceItemFromInstance(DomHelper helper, DomInstance domInstance, string label)
		{
			var instance = ServiceInstancesExtentions.GetTypedInstance(domInstance);
			var serviceItemToRemove = instance.GetServiceItems().FirstOrDefault(x => x.Label == label);
			if (serviceItemToRemove != null && !LinkedReferenceStillActive(serviceItemToRemove.ServiceItemType, serviceItemToRemove.ImplementationReference))
			{
				instance.GetServiceItems().Remove(serviceItemToRemove);

				var id = serviceItemToRemove.ServiceItemID?.ToString();
				var relationships = instance
					.GetServiceItemRelationships()
					.Where(r => r.ParentServiceItem == id || r.ChildServiceItem == id);

				relationships.ToList().ForEach(r => instance.GetServiceItemRelationships().Remove(r));

				instance.Save(helper);
			}
		}

		private bool LinkedBookingStillActive(Guid refId)
		{
			var rm = new ResourceManagerHelper(_engine.SendSLNetSingleResponseMessage);
			var reservation = rm.GetReservationInstance(refId);
			if (reservation.StartTimeUTC > DateTime.UtcNow
			    && (reservation.Status == ReservationStatus.Pending || reservation.Status == ReservationStatus.Confirmed))
			{
				rm.RemoveReservationInstances(reservation);
				return false;
			}

			if (reservation.EndTimeUTC < DateTime.UtcNow
			    || reservation.Status == ReservationStatus.Canceled
			    || reservation.Status == ReservationStatus.Ended)
			{
				return false;
			}

			throw new InvalidOperationException($"Booking '{reservation.Name}' still active on the system. Please finish this booking first before removing the service item from the inventory.");
		}

		private bool LinkedJobStillActive(Guid refId)
		{
			var schedulingHelper = new SchedulingHelper(_engine);
			var job = schedulingHelper.GetJob(refId);
			if (job == null)
			{
				return false; // If job doesn't exist, then it can't be active.
			}

			if (job.End < DateTime.UtcNow || job.Start > DateTime.UtcNow)
			{
				var cancelJobInputData = new ExecuteJobAction
				{
					DomJobId = job.Id,
					JobAction = Skyline.DataMiner.Utils.MediaOps.Common.IOData.Scheduling.Scripts.JobHandler.JobAction.CancelJob,
				};

				var cancelOutputData = cancelJobInputData.SendToJobHandler(_engine, true);
				if (!cancelOutputData.TraceData.HasSucceeded())
				{
					throw new InvalidOperationException($"Could not cancel Job '{refId}' due to : {JsonConvert.SerializeObject(cancelOutputData.TraceData)}");
				}

				var deleteJobInputData = new ExecuteJobAction
				{
					DomJobId = job.Id,
					JobAction = Skyline.DataMiner.Utils.MediaOps.Common.IOData.Scheduling.Scripts.JobHandler.JobAction.DeleteJob,
				};
				var deleteOutputData = deleteJobInputData.SendToJobHandler(_engine, true);

				if (!deleteOutputData.TraceData.HasSucceeded())
				{
					throw new InvalidOperationException($"Could not delete Job '{refId}' due to : {JsonConvert.SerializeObject(deleteOutputData.TraceData)}");
				}

				return false;
			}

			throw new InvalidOperationException($"Job '{refId}' still active on the system. Please finish this job first before removing the service item from the inventory.");
		}

		private bool LinkedReferenceStillActive(SlcServicemanagementIds.Enums.ServiceitemtypesEnum? serviceItemType, string implementationReference)
		{
			if (!Guid.TryParse(implementationReference, out Guid refId))
			{
				return false;
			}

			if (serviceItemType == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Workflow)
			{
				// Check job
				return LinkedJobStillActive(refId);
			}

			if (serviceItemType == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.SRMBooking)
			{
				// Check booking
				return LinkedBookingStillActive(refId);
			}

			if (serviceItemType == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Service)
			{
				// Check linked item
				return LinksStillExist(refId);
			}

			return false;
		}

		private bool LinksStillExist(Guid refId)
		{
			var linkHelper = new DataHelperLink(Engine.SLNetRaw);
			Models.Link link = linkHelper.Read().Find(x => x.ID == refId);
			if (link == null)
			{
				return false;
			}

			var dataHelper = new DataHelperService(Engine.SLNetRaw);

			FilterElement<Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement.Models.Service> filter = new ORFilterElement<Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement.Models.Service>();
			if (link.ChildID != null && Guid.TryParse(link.ChildID, out Guid childId))
			{
				filter = filter.OR(ServiceExposers.Guid.Equal(childId));
			}

			if (link.ParentID != null && Guid.TryParse(link.ParentID, out Guid parentId))
			{
				filter = filter.OR(ServiceExposers.Guid.Equal(parentId));
			}

			var services = dataHelper.Read(filter);
			if (services.Count > 1)
			{
				return true;
			}

			return false;
		}

		private void RunSafe()
		{
			string domIdRaw = _engine.GetScriptParam("DOM ID").Value;
			Guid domId = JsonConvert.DeserializeObject<List<Guid>>(domIdRaw).FirstOrDefault();
			if (domId == Guid.Empty)
			{
				throw new InvalidOperationException("No DOM ID provided as input to the script");
			}

			string serviceItemLabelRaw = _engine.GetScriptParam("Service Item Label").Value;
			string serviceItemLabel = JsonConvert.DeserializeObject<List<string>>(serviceItemLabelRaw).FirstOrDefault()
			                          ?? throw new InvalidOperationException("No Service Item Label provided as input to the script");

			var domHelper = new DomHelper(_engine.SendSLNetMessages, SlcServicemanagementIds.ModuleId);
			var domInstance = domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(domId)).FirstOrDefault()
			                  ?? throw new InvalidOperationException($"No DOM Instance with ID '{domId}' found on the system!");

			DeleteServiceItemFromInstance(domHelper, domInstance, serviceItemLabel);
			throw new ScriptAbortException("OK");
		}
	}
}