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
namespace SLC_SM_IAS_Add_Service_Order_Item_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Library;
	using Library.Views;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	using SLC_SM_Common.API.ServiceManagementApi;

	using SLC_SM_IAS_Add_Service_Order_Item_1.Presenters;
	using SLC_SM_IAS_Add_Service_Order_Item_1.Views;

	/// <summary>
	///     Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		private InteractiveController _controller;
		private IEngine _engine;

		private enum Action
		{
			Add,
			Edit,
		}

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
			if (engine.IsInteractive)
			{
				engine.FindInteractiveClient("Failed to run script in interactive mode", 1);
			}

			try
			{
				_engine = engine;
				_controller = new InteractiveController(engine);
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

		private static void AddOrUpdateServiceItemToInstance(Repo helper, Models.ServiceOrder instance, Models.ServiceOrderItems updatedData)
		{
			helper.ServiceOrderItems.CreateOrUpdate(updatedData.ServiceOrderItem);

			var existingItem = instance.OrderItems.FirstOrDefault(x => x?.ServiceOrderItem?.ID == updatedData.ServiceOrderItem.ID);
			if (existingItem != null)
			{
				// Already linked - nothing to do
				return;
			}

			instance.OrderItems.Add(updatedData);
			helper.ServiceOrders.CreateOrUpdate(instance);
		}

		private static string[] GetServiceItemLabels(Models.ServiceOrder serviceOrdersInstance, string oldLbl)
		{
			var items = serviceOrdersInstance.OrderItems.Where(x => x?.ServiceOrderItem != null).Select(x => x.ServiceOrderItem.Name).ToList();

			items.Remove(oldLbl);
			return items.ToArray();
		}

		private void RunSafe()
		{
			string domIdRaw = _engine.GetScriptParam("DOM ID").Value;
			Guid domId = JsonConvert.DeserializeObject<List<Guid>>(domIdRaw).FirstOrDefault();
			if (domId == Guid.Empty)
			{
				throw new InvalidOperationException("No DOM ID provided as input to the script");
			}

			string actionRaw = _engine.GetScriptParam("Action").Value.Trim('"', '[', ']');
			if (!Enum.TryParse(actionRaw, true, out Action action))
			{
				throw new InvalidOperationException("No Action provided as input to the script");
			}

			var repo = new Repo(Engine.SLNetRaw);
			var order = repo.ServiceOrders.Read().Find(x => x.ID == domId)
				?? throw new InvalidOperationException($"No DOM Instance with ID '{domId}' found on the system.");

			Guid.TryParse(_engine.GetScriptParam("Service Order Item ID").Value.Trim('"', '[', ']'), out Guid orderItemid);

			var orderItem = order.OrderItems.FirstOrDefault(x => x.ServiceOrderItem?.ID == orderItemid);

			// Init views
			var view = new ServiceOrderItemView(_engine);
			var presenter = new ServiceOrderItemPresenter(view, repo, GetServiceItemLabels(order, orderItem?.ServiceOrderItem.Name));

			// Events
			view.BtnCancel.Pressed += (sender, args) => throw new ScriptAbortException("OK");
			view.BtnAdd.Pressed += (sender, args) =>
			{
				if (presenter.Validate())
				{
					AddOrUpdateServiceItemToInstance(repo, order, presenter.GetData);
					throw new ScriptAbortException("OK");
				}
			};

			if (action == Action.Add)
			{
				presenter.LoadFromModel(order.OrderItems.Count(x => x.ServiceOrderItem != null));
			}
			else
			{
				presenter.LoadFromModel(orderItem);
			}

			// Run interactive
			_controller.ShowDialog(view);
		}
	}
}
