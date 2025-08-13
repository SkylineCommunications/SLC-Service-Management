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

05/08/2025	1.0.0.1		RME, Skyline	Initial version
****************************************************************************
*/

namespace SLCSMDemo
{
	using System;
	using System.Linq;

	using DomHelpers.SlcConfigurations;
	using DomHelpers.SlcServicemanagement;

	using Library;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	using Models = Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement.Models;

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
				engine.ExitFail("Run|Something went wrong: " + e);
			}
		}

		private void RunSafe(IEngine engine)
		{
			//FilterServiceOnCategory(engine);

			FilterServiceOnCharacteristic(engine);
		}

		////private static void FilterServiceOnCategory(IEngine engine)
		////{
		////	string categoryType = "Channel";
		////	string categoryName = "ARD";

		////	var dataHelpers = new DataHelpersServiceManagement(Engine.SLNetRaw);

		////	var categoryToMatch = dataHelpers.ServiceCategories.Read().Find(x => x.Type == categoryType && x.Name == categoryName)
		////	                      ?? throw new InvalidOperationException($"No Category found matching '{categoryType}-{categoryName}'");

		////	var services = dataHelpers.Services.Read(ServicesInstanceExposers.ServiceInfoSection.ServiceCategory.Equal(categoryToMatch));
		////	engine.GenerateInformation($"Service(s) found:\r\n{String.Join(Environment.NewLine, services.Select(s => $"{s.Name} ({s.ID})"))}");
		////}

		private static void FilterServiceOnCharacteristic(IEngine engine)
		{
			string configurationParameter = "Service Type";
			//string configurationParameterLabel = "Service Type";
			string configurationParameterValue = "Channel";

			var dataHelpersSrvMgmt = new DataHelpersServiceManagement(Engine.SLNetRaw);
			var services = dataHelpersSrvMgmt.Services.GetServicesByCharacteristic(configurationParameter, null, configurationParameterValue);

			engine.GenerateInformation($"Service(s) found:\r\n{String.Join(Environment.NewLine, services.Select(s => $"{s.Name} ({s.ID})"))}");
		}
	}
}
