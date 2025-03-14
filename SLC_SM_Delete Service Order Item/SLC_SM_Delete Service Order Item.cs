//---------------------------------
// SLC_SM_Delete Service Order Item_1.cs
//---------------------------------
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
namespace SLC_SM_Delete_Service_Order_Item_1
{
	using System;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <summary>
	///     Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		private IEngine _engine;

		/// <summary>
		///     The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			try
			{
				_engine = engine;
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
				engine.ExitFail(e.Message);
			}
		}

		private static void DeleteServiceItemFromInstance(DomHelper helper, ServiceOrdersInstance domInstance, Guid serviceOrderItemId)
		{
			var itemToRemove = domInstance.ServiceOrderItems.FirstOrDefault(x => x.ServiceOrderItem == serviceOrderItemId);
			if (itemToRemove == null)
			{
				throw new InvalidOperationException($"No Service order item exists with ID '{serviceOrderItemId}' to remove");
			}

			RemoveLinkedItems(helper, itemToRemove);

			domInstance.ServiceOrderItems.Remove(itemToRemove);
			domInstance.Save(helper);
		}

		private static void RemoveLinkedItems(DomHelper helper, ServiceOrderItemsSection serviceOrderItem)
		{
			if (!serviceOrderItem.ServiceOrderItem.HasValue)
			{
				return;
			}

			var serviceOderItemToDelete = helper.DomInstances.Read(DomInstanceExposers.Id.Equal(serviceOrderItem.ServiceOrderItem.Value)).FirstOrDefault();
			if (serviceOderItemToDelete == null)
			{
				return;
			}

			helper.DomInstances.Delete(serviceOderItemToDelete);

			var inst = new ServiceOrderItemsInstance(serviceOderItemToDelete);
			if (inst.ServiceOrderItemServiceInfo.Configuration.HasValue)
			{
				var configToDelete = helper.DomInstances.Read(DomInstanceExposers.Id.Equal(inst.ServiceOrderItemServiceInfo.Configuration.Value)).FirstOrDefault();
				if (configToDelete == null)
				{
					return;
				}

				helper.DomInstances.Delete(configToDelete);
			}

			if (inst.ServiceOrderItemServiceInfo.Properties.HasValue)
			{
				var propToDelete = helper.DomInstances.Read(DomInstanceExposers.Id.Equal(inst.ServiceOrderItemServiceInfo.Properties.Value)).FirstOrDefault();
				if (propToDelete == null)
				{
					return;
				}

				helper.DomInstances.Delete(propToDelete);
			}
		}

		private void RunSafe()
		{
			if (!Guid.TryParse(_engine.GetScriptParam("DOM ID").Value.Trim('"', '[', ']'), out Guid domId))
			{
				throw new InvalidOperationException("No DOM ID provided as input to the script");
			}

			if (!Guid.TryParse(_engine.GetScriptParam("Service Order Item ID").Value.Trim('"', '[', ']'), out Guid serviceOrderItemId))
			{
				throw new InvalidOperationException("No Service Order Item ID provided as input to the script");
			}

			var domHelper = new DomHelper(_engine.SendSLNetMessages, SlcServicemanagementIds.ModuleId);
			var domInstance = domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(domId)).FirstOrDefault()
							  ?? throw new InvalidOperationException($"No DOM Instance with ID '{domId}' found on the system!");
			var orderItemInstance = new ServiceOrdersInstance(domInstance);

			DeleteServiceItemFromInstance(domHelper, orderItemInstance, serviceOrderItemId);
			throw new ScriptAbortException("OK");
		}
	}
}