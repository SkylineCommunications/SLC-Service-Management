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
namespace SLC_SM_Create_Service_Inventory_Item
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;

	using Library;
	using Library.Views;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	using SLC_SM_Create_Service_Inventory_Item.Presenters;
	using SLC_SM_Create_Service_Inventory_Item.Views;

	/// <summary>
	///     Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		private InteractiveController _controller;
		private DomHelper _domHelper;
		private IEngine _engine;

		private enum Action
		{
			Add,
			AddItem,
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
				InitHelpers();

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

		private void AddOrUpdateService(DataHelpersServiceManagement repo, Models.Service instance)
		{
			if (!instance.ServiceSpecificationId.HasValue || instance.ServiceSpecificationId == Guid.Empty)
			{
				repo.Services.CreateOrUpdate(instance);
				return;
			}

			var serviceSpecificationInstance = repo.ServiceSpecifications.Read().First(x => x.ID == instance.ServiceSpecificationId);

			instance.Icon = serviceSpecificationInstance.Icon;
			instance.Description = serviceSpecificationInstance.Description;
			instance.Properties = serviceSpecificationInstance.Properties ?? new Models.ServicePropertyValues();
			instance.Properties.ID = Guid.NewGuid();

			if (serviceSpecificationInstance.Configurations != null)
			{
				instance.Configurations = serviceSpecificationInstance.Configurations
					.Where(x => x?.ConfigurationParameter != null)
					.Select(
						x =>
						{
							var scv = new Models.ServiceConfigurationValue
							{
								ConfigurationParameter = x.ConfigurationParameter,
								Mandatory = x.MandatoryAtService,
							};
							scv.ConfigurationParameter.ID = Guid.Empty;
							return scv;
						})
					.ToList();
			}

			if (serviceSpecificationInstance.ServiceItemsRelationships != null)
			{
				foreach (var relationship in serviceSpecificationInstance.ServiceItemsRelationships)
				{
					if (!instance.ServiceItemsRelationships.Contains(relationship))
					{
						instance.ServiceItemsRelationships.Add(relationship);
					}
				}
			}

			if (serviceSpecificationInstance.ServiceItems != null)
			{
				foreach (var item in serviceSpecificationInstance.ServiceItems)
				{
					if (!instance.ServiceItems.Contains(item))
					{
						if (instance.ServiceItems.Any(x => x.ID == item.ID))
						{
							item.ID = instance.ServiceItems.Max(x => x.ID) + 1;
						}

						if (String.IsNullOrEmpty(item.Label))
						{
							item.Label = $"Service Item #{item.ID:000}";
						}

						if (String.IsNullOrEmpty(item.DefinitionReference))
						{
							item.DefinitionReference = String.Empty;
						}

						if (String.IsNullOrEmpty(item.Script))
						{
							item.Script = String.Empty;
						}

						instance.ServiceItems.Add(item);
					}
				}
			}

			repo.Services.CreateOrUpdate(instance);
		}

		private void CreateNewServiceAndLinkItToServiceOrder(DataHelpersServiceManagement repo, Models.ServiceOrderItem serviceOrder)
		{
			List<Models.Service> services = repo.Services.Read();
			if (serviceOrder.ServiceId.HasValue && services.Exists(s => s.ID == serviceOrder.ServiceId))
			{
				// Already initialized - don't do anything, safety check
				return;
			}

			// Create new service item based on order
			Models.Service newService = new Models.Service
			{
				ServiceID = repo.Services.UniqueServiceId(services),
				Name = serviceOrder.Name,
				Description = serviceOrder.Name,
				StartTime = serviceOrder.StartTime,
				EndTime = serviceOrder.EndTime,
				Icon = String.Empty,
				ServiceSpecificationId = serviceOrder.SpecificationId,
				Properties = serviceOrder.Properties,
				Category = repo.ServiceCategories.Read().Find(x => x.ID == serviceOrder.ServiceCategoryId),
				ServiceItems = new List<Models.ServiceItem>(),
				ServiceItemsRelationships = new List<Models.ServiceItemRelationShip>(),
			};

			if (serviceOrder.Configurations != null)
			{
				newService.Configurations = serviceOrder.Configurations
					.Where(x => x?.ConfigurationParameter != null)
					.Select(
						x =>
						{
							var scv = new Models.ServiceConfigurationValue
							{
								ConfigurationParameter = x.ConfigurationParameter,
								Mandatory = x.Mandatory,
							};
							scv.ConfigurationParameter.ID = Guid.Empty;
							return scv;
						})
					.ToList();
			}

			var spec = repo.ServiceSpecifications.Read().Find(x => x.ID == serviceOrder.SpecificationId);
			if (spec != null)
			{
				newService.Icon = spec.Icon;
				if (spec.ServiceItemsRelationships != null)
				{
					foreach (var relationship in spec.ServiceItemsRelationships)
					{
						if (!newService.ServiceItemsRelationships.Contains(relationship))
						{
							newService.ServiceItemsRelationships.Add(relationship);
						}
					}
				}

				if (spec.ServiceItems != null)
				{
					foreach (var item in spec.ServiceItems)
					{
						if (!newService.ServiceItems.Contains(item))
						{
							if (newService.ServiceItems.Any(x => x.ID == item.ID))
							{
								item.ID = newService.ServiceItems.Max(x => x.ID) + 1;
							}

							if (String.IsNullOrEmpty(item.Label))
							{
								item.Label = $"Service Item #{item.ID:000}";
							}

							if (String.IsNullOrEmpty(item.DefinitionReference))
							{
								item.DefinitionReference = String.Empty;
							}

							if (String.IsNullOrEmpty(item.Script))
							{
								item.Script = String.Empty;
							}

							newService.ServiceItems.Add(item);
						}
					}
				}
			}

			var dataHelperService = new DataHelperService(Engine.SLNetRaw);
			Guid newServiceId = dataHelperService.CreateOrUpdate(newService);

			// Provide link on Service Order
			serviceOrder.ServiceId = newServiceId;
			repo.ServiceOrderItems.CreateOrUpdate(serviceOrder);

			// Update state
			var domInstanceId = new DomInstanceId(serviceOrder.ID);
			if (serviceOrder.StatusId == SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Statuses.New)
			{
				_domHelper.DomInstances.DoStatusTransition(domInstanceId, SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.New_To_Acknowledged);
				_domHelper.DomInstances.DoStatusTransition(domInstanceId, SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.Acknowledged_To_Inprogress);
			}

			if (serviceOrder.StatusId == SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Statuses.Acknowledged)
			{
				_domHelper.DomInstances.DoStatusTransition(domInstanceId, SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.Acknowledged_To_Inprogress);
			}

			// Update state of main Service Order as well
			Models.ServiceOrder order = repo.ServiceOrders.Read().Find(x => x.OrderItems.Exists(o => o.ServiceOrderItem.ID == serviceOrder.ID));
			if (order != null)
			{
				var orderId = new DomInstanceId(order.ID);
				if (order.StatusId == SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Statuses.New)
				{
					_domHelper.DomInstances.DoStatusTransition(orderId, SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.New_To_Acknowledged);
					_domHelper.DomInstances.DoStatusTransition(orderId, SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Acknowledged_To_Inprogress);
				}

				if (order.StatusId == SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Statuses.Acknowledged)
				{
					_domHelper.DomInstances.DoStatusTransition(orderId, SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Acknowledged_To_Inprogress);
				}
			}
		}

		private Models.Service GetService(DataHelpersServiceManagement repo, Guid domId)
		{
			if (domId == Guid.Empty)
			{
				throw new InvalidOperationException("No existing DOM ID was provided as script input!");
			}

			return repo.Services.Read().Find(x => x.ID == domId)
			       ?? throw new InvalidOperationException($"No Dom Instance with ID '{domId}' found on the system!");
		}

		private void InitHelpers()
		{
			_domHelper = new DomHelper(_engine.SendSLNetMessages, SlcServicemanagementIds.ModuleId);
		}

		private void RunSafe()
		{
			string actionRaw = _engine.GetScriptParam("Action").Value.Trim('"', '[', ']');
			if (!Enum.TryParse(actionRaw, true, out Action action))
			{
				action = Action.AddItem;
			}

			string domIdRaw = _engine.GetScriptParam("DOM ID").Value.Trim('"', '[', ']');
			Guid.TryParse(domIdRaw, out Guid domId);

			var repo = new DataHelpersServiceManagement(Engine.SLNetRaw);

			// Init views
			var view = new ServiceView(_engine);
			var presenter = new ServicePresenter(repo, view, repo.Services.Read().Select(x => x.Name).ToList());

			if (action == Action.AddItem)
			{
				var d = new MessageDialog(_engine, "Create Service Inventory Item from the selected service order item?") { Title = "Create Service Inventory Item From Order Item" };
				d.OkButton.Pressed += (sender, args) =>
				{
					var serviceOrderItem = repo.ServiceOrderItems.Read().Find(x => x.ID == domId);
					if (domId != Guid.Empty && serviceOrderItem == null)
					{
						throw new InvalidOperationException($"No Service Order Item with ID '{domId}' found on the system!");
					}

					CreateNewServiceAndLinkItToServiceOrder(repo, serviceOrderItem);
					throw new ScriptAbortException("OK");
				};
				_controller.ShowDialog(d);
			}
			else if (action == Action.Add)
			{
				presenter.LoadFromModel();
				view.BtnAdd.Pressed += (sender, args) =>
				{
					if (presenter.Validate())
					{
						AddOrUpdateService(repo, presenter.Instance);
						throw new ScriptAbortException("OK");
					}
				};
			}
			else
			{
				view.BtnAdd.Text = "Edit Service";
				presenter.LoadFromModel(GetService(repo, domId));
				view.BtnAdd.Pressed += (sender, args) =>
				{
					if (presenter.Validate())
					{
						repo.Services.CreateOrUpdate(presenter.Instance); // Only update service info
						throw new ScriptAbortException("OK");
					}
				};
			}

			// Events
			view.BtnCancel.Pressed += (sender, args) => throw new ScriptAbortException("OK");

			// Run interactive
			_controller.ShowDialog(view);
		}
	}
}