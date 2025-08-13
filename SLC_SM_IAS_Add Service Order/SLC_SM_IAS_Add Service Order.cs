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
namespace SLC_SM_IAS_Add_Service_Order_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Library.Views;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;

	using SLC_SM_IAS_Add_Service_Order_1.Presenters;
	using SLC_SM_IAS_Add_Service_Order_1.Views;

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

		private void RunSafe()
		{
			Guid.TryParse(_engine.GetScriptParam("DOM ID").Value.Trim('"', '[', ']'), out Guid domId);

			string actionRaw = _engine.GetScriptParam("Action").Value.Trim('"', '[', ']');
			if (!Enum.TryParse(actionRaw, true, out Action action))
			{
				throw new InvalidOperationException("No Action provided as input to the script");
			}

			var dataHelperOrders = new DataHelperServiceOrder(Engine.SLNetRaw);
			List<Models.ServiceOrder> serviceOrders = dataHelperOrders.Read();

			var usedOrderItemLabels = serviceOrders.Select(o => o.Name).ToList();
			var usedOrderIds = serviceOrders.Select(o => o.OrderId).ToList();

			// Init views
			var view = new ServiceOrderView(_engine);
			var presenter = new ServiceOrderPresenter(_engine, view, usedOrderItemLabels, usedOrderIds);

			// Events
			view.BtnCancel.Pressed += (sender, args) => throw new ScriptAbortException("OK");
			view.BtnAdd.Pressed += (sender, args) =>
			{
				if (presenter.Validate())
				{
					dataHelperOrders.CreateOrUpdate(presenter.GetData);
					throw new ScriptAbortException("OK");
				}
			};

			if (action == Action.Add)
			{
				presenter.LoadFromModel();
			}
			else
			{
				var ordersInstance = serviceOrders.Find(x => x.ID == domId)
				                     ?? throw new InvalidOperationException($"No Service Order with ID '{domId}' found on the system!");
				presenter.LoadFromModel(ordersInstance);
			}

			// Run interactive
			_controller.ShowDialog(view);
		}
	}
}
