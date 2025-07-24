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

namespace Launch_Interactive_Subscript_1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DomHelpers.SlcServicemanagement;
    using Newtonsoft.Json;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    public class Script
    {
        /// <summary>
        /// The script entry point.
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
                string scriptNameParam = engine.GetScriptParam("Script Name").Value;
                string scriptName = JsonConvert.DeserializeObject<List<string>>(scriptNameParam).FirstOrDefault()
                                    ?? throw new InvalidOperationException("No Service Item Script provided to run the action");

                string bookingManagerElementName = JsonConvert.DeserializeObject<List<string>>(engine.GetScriptParam("Booking Manager Element Name").Value).FirstOrDefault()
                                                   ?? throw new InvalidOperationException("No Booking Manager Reference provided to run the action");

                string scriptOutput = RunScript(engine, scriptName, bookingManagerElementName);

                if (Guid.TryParse(engine.GetScriptParam("DOM ID").Value.Trim('"', '[', ']'), out Guid domId))
                {
                    string itemLabel = engine.GetScriptParam("Item Label").Value.Trim('"', '[', ']');

                    var domHelper = new DomHelper(engine.SendSLNetMessages, SlcServicemanagementIds.ModuleId);
                    var inst = domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(domId)).FirstOrDefault()
                               ?? throw new InvalidOperationException($"No DOM Instance with ID '{domId}' found on the system");
                    if (inst.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.Services.Id)
                    {
                        var serviceItemInstance = new ServicesInstance(inst);
                        var serviceItem = serviceItemInstance.ServiceItems.FirstOrDefault(x => x.Label == itemLabel);
                        if (serviceItem == null)
                        {
                            return;
                        }

                        serviceItem.ImplementationReference = !String.IsNullOrEmpty(scriptOutput) ? scriptOutput : "Reference Unknown";
                        serviceItemInstance.Save(domHelper);
                    }
                }
            }
            catch (Exception e)
            {
                engine.ExitFail(e.Message);
            }
        }

        private static string RunScript(IEngine engine, string scriptName, string bookingManagerElementName)
        {
            var subScript = engine.PrepareSubScript(scriptName);
            subScript.Synchronous = true;
            subScript.ExtendedErrorInfo = true;
            subScript.InheritScriptOutput = true;

            subScript.SelectScriptParam("Booking Manager Element Info", "{ \"Element\":\"" + bookingManagerElementName + "\",\"TableIndex\":\"\",\"Action\":\"New\"}");

            subScript.StartScript();

            if (subScript.HadError)
            {
                throw new InvalidOperationException($"Failed to start the Booking Manager script '{scriptName}' due to:\r\n" + String.Join(@"\r\n ->", subScript.GetErrorMessages()));
            }

            return subScript.GetScriptResult().FirstOrDefault().Value;
        }
    }
}
