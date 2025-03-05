
//---------------------------------
// SLC_SM_Create Service Inventory Item_1.cs
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
namespace SLC_SM_Create_Service_Inventory_Item_1
{
    using System;
    using System.Linq;
    using DomHelpers.SlcServicemanagement;
    using Library;
    using Library.Views;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using SLC_SM_Create_Service_Inventory_Item_1.Presenters;
    using SLC_SM_Create_Service_Inventory_Item_1.Views;

    /// <summary>
    ///     Represents a DataMiner Automation script.
    /// </summary>
    public class Script
    {
        private InteractiveController _controller;
        private IEngine _engine;
        private DomHelper _domHelper;

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
                InitHelpers();

                RunSafe();
            }
            catch (ScriptAbortException)
            {
                // Catch normal abort exceptions (engine.ExitFail or engine.ExitSuccess)
                throw; // Comment if it should be treated as a normal exit of the script.
            }
            catch (ScriptForceAbortException)
            {
                // Catch forced abort exceptions, caused via external maintenance messages.
                throw;
            }
            catch (ScriptTimeoutException)
            {
                // Catch timeout exceptions for when a script has been running for too long.
                throw;
            }
            catch (InteractiveUserDetachedException)
            {
                // Catch a user detaching from the interactive script by closing the window.
                // Only applicable for interactive scripts, can be removed for non-interactive scripts.
                throw;
            }
            catch (Exception e)
            {
                var errorView = new ErrorView(engine, "Error", e.Message, e.ToString());
                _controller.ShowDialog(errorView);
            }
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
                throw new InvalidOperationException("No Action provided as input to the script");
            }

            Guid.TryParse(_engine.GetScriptParam("DOM ID").Value.Trim('"', '[', ']'), out Guid domId);

            var repo = new Repo(_domHelper);

            // Init views
            var view = new ServiceView(_engine);
            var presenter = new ServicePresenter(_engine, repo, view, repo.AllServices.Select(x => new ServicesInstance(x).Name).ToArray());

            // Events
            view.BtnCancel.Pressed += (sender, args) => throw new ScriptAbortException("OK");
            view.BtnAdd.Pressed += (sender, args) =>
            {
                if (presenter.Validate())
                {
                    AddOrUpdateService(presenter.Instance);
                    throw new ScriptAbortException("OK");
                }
            };

            if (action == Action.Add)
            {
                if (domId != Guid.Empty)
                {
                    ServicesInstance newServiceInstance = CreateNewServiceAndLinkItToServiceOrder(domId);
                    presenter.LoadFromModel(newServiceInstance);
                }
                else
                {
                    presenter.LoadFromModel();
                }
            }
            else
            {
                view.Title = "Manage Service";
                view.BtnAdd.Text = "Edit Service";
                presenter.LoadFromModel(GetServiceItemSection(domId));
            }

            // Run interactive
            _controller.ShowDialog(view);
        }

        private ServicesInstance CreateNewServiceAndLinkItToServiceOrder(Guid domId)
        {
            var instance = _domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(domId)).FirstOrDefault()
                ?? throw new InvalidOperationException($"No DOM Instance with ID '{domId}' found on the system!");

            if (instance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceOrderItems.Id)
            {
                var serviceOrderInstance = new ServiceOrderItemsInstance(instance);
                if (serviceOrderInstance.ServiceOrderItemServiceInfo.Service.HasValue)
                {
                    var inst = _domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(serviceOrderInstance.ServiceOrderItemServiceInfo.Service.Value)).FirstOrDefault()
                           ?? throw new InvalidOperationException($"No Dom Instance with ID '{serviceOrderInstance.ServiceOrderItemServiceInfo.Service.Value}' found on the system!");
                    return new ServicesInstance(inst);
                }

                // Create new service item based on order
                var newService = new ServicesInstance();
                newService.ServiceInfo.ServiceName = serviceOrderInstance.ServiceOrderItemInfo.Name;
                newService.ServiceInfo.Description = serviceOrderInstance.ServiceOrderItemInfo.Name;
                newService.ServiceInfo.Icon = String.Empty;
                newService.ServiceInfo.ServiceSpecifcation = serviceOrderInstance.ServiceOrderItemServiceInfo.ServiceSpecification;
                AddOrUpdateService(newService);

                // Provide link
                serviceOrderInstance.ServiceOrderItemServiceInfo.Service = newService.ID.Id;
                serviceOrderInstance.Save(_domHelper);
                return newService;
            }

            throw new InvalidOperationException("Creating Service from this definition not supported yet");
        }

        private ServicesInstance GetServiceItemSection(Guid domId)
        {
            if (domId == Guid.Empty)
            {
                throw new InvalidOperationException("No existing DOM ID was provided as script input!");
            }

            var instance = _domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(domId)).FirstOrDefault()
                           ?? throw new InvalidOperationException($"No Dom Instance with ID '{domId}' found on the system!");
            return new ServicesInstance(instance);
        }

        private void AddOrUpdateService(ServicesInstance instance)
        {
            if (!instance.ServiceInfo.ServiceSpecifcation.HasValue || instance.ServiceInfo.ServiceSpecifcation == Guid.Empty)
            {
                if (!instance.ServiceItems.Any())
                {
                    instance.ServiceItems.Add(new ServiceItemsSection());
                }

                if (!instance.ServiceItemRelationship.Any())
                {
                    instance.ServiceItemRelationship.Add(new ServiceItemRelationshipSection());
                }

                instance.Save(_domHelper);
                return;
            }

            var domInstance = _domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(instance.ServiceInfo.ServiceSpecifcation.Value)).FirstOrDefault()
                              ?? throw new InvalidOperationException($"No Service Specification found with ID '{instance.ServiceInfo.ServiceSpecifcation}'.");
            var serviceSpecificationInstance = new ServiceSpecificationsInstance(domInstance);

            instance.ServiceInfo.Icon = serviceSpecificationInstance.ServiceSpecificationInfo.Icon;
            instance.ServiceInfo.Description = serviceSpecificationInstance.ServiceSpecificationInfo.Description;
            instance.ServiceInfo.ServiceProperties = serviceSpecificationInstance.ServiceSpecificationInfo.ServiceProperties;
            instance.ServiceInfo.ServiceConfiguration = serviceSpecificationInstance.ServiceSpecificationInfo.ServiceConfiguration;

            foreach (var relationship in serviceSpecificationInstance.ServiceItemRelationship)
            {
                if (!instance.ServiceItemRelationship.Contains(relationship))
                {
                    instance.ServiceItemRelationship.Add(relationship);
                }
            }

            foreach (var item in serviceSpecificationInstance.ServiceItems)
            {
                if (!instance.ServiceItems.Contains(item))
                {
                    instance.ServiceItems.Add(item);
                }
            }

            instance.Save(_domHelper);
        }
    }
}
