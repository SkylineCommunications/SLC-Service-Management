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

06/06/2025	1.0.0.1		RME, Skyline	Initial version
****************************************************************************
*/
namespace SLC_SM_AS_Ingest_Acquisition_Data
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using ACQ_LIB;

	using DomHelpers.SlcConfigurations;
	using DomHelpers.SlcServicemanagement;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;

	using Models = Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement.Models;

	/// <summary>
	///     Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		private const string NamePolarization = "Polarization";
		private const string NameOrbitalPosition = "Orbital Position";
		private const string NameServiceId = "Service ID";
		private const string NameTransponder = "Transponder";
		private const string NameUrl = "URL";
		private const string NameMulticast = "Multicast Address";
		private const string NameSourceIp = "Source IP";
		private const string NameUrlMain = "URL Main";
		private const string NameUrlBackup = "URL Backup";
		private const string NameMulticastMain = "Multicast Main";
		private const string NameMulticastBackup = "Multicast Backup";
		private const string NameSourceIpMain = "Source IP Main";
		private const string NameSourceIpBackup = "Source IP Backup";
		private const string NameInterface = "Interface";
		private const string NameDevice = "Device Name";
		private const string NameInterfaceMain = "Interface Main";
		private const string NameInterfaceBackup = "Interface Backup";
		private const string NameDeviceBackup = "Device Backup";

		private static readonly string AcquisitionData1 =
			"{\"id\":\"VFCZ:4440\",\"referenceProgramIds\":[9259,9175],\"name\":\"BBC_News_Europe/SAT/1/V02\",\"tenant\":\"VFCZ\",\"sourceId\":\"f3e32bb68f124c84e054304dc0cb042da969fa3c\",\"sourceType\":\"SAT\",\"details\":{\"usecase\":\"SATDL - FTA Channels\",\"_id\":\"682de70f23a1002e1bc6007c\",\"updated_at\":\"2025-06-03T15:17:58+02:00\",\"created_at\":\"2025-06-03T15:17:58+02:00\",\"type\":\"SAT\",\"complete\":true,\"main\":true,\"encrypted\":false,\"is_radio\":false,\"eit\":false,\"eit_pf\":false,\"transparent_mpts\":false,\"service_type\":22,\"service_type_label\":\"AVCSDTV (0x16)\",\"tenant_ids\":[1,2,6],\"service_provider\":[639],\"usage_type\":\"ALL\",\"sid_1\":13104,\"sid_2\":null,\"sid_3\":null,\"sid_4\":null,\"source_asi\":null,\"source_fm\":null,\"source_dvbt2\":null,\"source_ip\":null,\"source_sat\":{\"guid\":\"f3e32bb68f124c84e054304dc0cb042da969fa3c\",\"updated_at\":\"2025-06-03T15:17:35+02:00\",\"created_at\":\"2025-06-03T15:17:35+02:00\",\"orbital_position\":\"13,0° Ost; Hot Bird 13F\",\"polarisation\":\"V\",\"frequency\":11727,\"symbol_rate\":29900,\"transponder\":\"50\",\"standard\":\"DVB-S2\",\"modulation\":\"8PSK\",\"fec\":\"3/4\",\"nid\":318,\"tid\":5000,\"isi\":null,\"gsi\":null,\"sid\":13104},\"source_sdi\":null,\"source_srt\":null,\"source_stream\":null,\"programs\":[9259,9175],\"additional_information\":null,\"pids\":{\"video\":1040,\"pmt\":104,\"pcr\":1040,\"teletext\":1042,\"subtitle\":null,\"ait_hbbtv\":0,\"eit\":0,\"sdt\":0,\"audio\":[]},\"notice\":null},\"readyForProvision\":false,\"usageType\":\"ALL\",\"createdAt\":\"2025-05-21T14:45:35.632Z\",\"updatedAt\":\"2025-05-21T14:45:35.632Z\",\"programs\":[]}";
		private static readonly string AcquisitionData2 =
			"{\"id\":\"VFDE:4432\",\"referenceProgramIds\":[1164],\"name\":\"Antenne_Thueringen_(Mitte)/STREAM/1/V02\",\"tenant\":\"VFDE\",\"sourceId\":\"854b0551a1327a9198ade5b37718a18bdc4a41ce\",\"sourceType\":\"Stream\",\"details\":{\"usecase\":\"Qbit Ipstream Radio Channels\",\"_id\":\"682de70f23a1002e1bc60312\",\"updated_at\":\"2025-06-03T15:17:58+02:00\",\"created_at\":\"2025-06-03T15:17:58+02:00\",\"type\":\"Stream\",\"complete\":true,\"main\":true,\"encrypted\":false,\"is_radio\":true,\"eit\":false,\"eit_pf\":false,\"transparent_mpts\":false,\"service_type\":2,\"service_type_label\":\"Radio (0x02)\",\"tenant_ids\":[1],\"service_provider\":[],\"usage_type\":\"ALL\",\"sid_1\":0,\"sid_2\":null,\"sid_3\":null,\"sid_4\":null,\"source_asi\":null,\"source_fm\":null,\"source_dvbt2\":null,\"source_ip\":null,\"source_sat\":null,\"source_sdi\":null,\"source_srt\":null,\"source_stream\":{\"guid\":\"854b0551a1327a9198ade5b37718a18bdc4a41ce\",\"updated_at\":\"2025-06-03T15:17:58+02:00\",\"created_at\":\"2025-06-03T15:17:58+02:00\",\"url_1a\":\"http://dist-01.audiomediaplus.de/atmitte_256k\",\"location_1a\":{\"id\":989,\"region\":\"OTHER\",\"name\":\"PoC Kerpen\",\"loc_type\":4,\"loc_type_label\":\"Foreign site\",\"host_prefix\":\"bm-mich\",\"address\":null,\"room\":null,\"postcode\":null,\"city\":null},\"url_1b\":\"http://dist-01.audiomediaplus.de/atmitte_256k\",\"location_1b\":{\"id\":1024,\"region\":\"OTHER\",\"name\":\"PoC Rödelheim\",\"loc_type\":4,\"loc_type_label\":\"Foreign site\",\"host_prefix\":\"f-brei\",\"address\":null,\"room\":null,\"postcode\":null,\"city\":null},\"url_2a\":null,\"location_2a\":null,\"url_2b\":null,\"location_2b\":null},\"programs\":[1164],\"additional_information\":null,\"pids\":{\"video\":0,\"pmt\":0,\"pcr\":0,\"teletext\":0,\"subtitle\":null,\"ait_hbbtv\":0,\"eit\":null,\"sdt\":null,\"audio\":[]},\"notice\":null},\"readyForProvision\":false,\"usageType\":\"ALL\",\"createdAt\":\"2025-05-21T14:45:35.645Z\",\"updatedAt\":\"2025-05-21T14:45:35.645Z\",\"programs\":[]}";
		private static readonly string AcquisitionData3 =
			"{\"id\":\"VFDE:3312\",\"referenceProgramIds\":[434,7895],\"name\":\"ARD_alpha_HD/LTGA3/7.2B\",\"tenant\":\"VFDE\",\"sourceId\":\"88879149e783e554601e2089f06165085bb206cc\",\"sourceType\":\"SDI\",\"details\":{\"usecase\":\"SDI Backhaul Channels\",\"_id\":\"682de70f23a1002e1bc600ae\",\"updated_at\":\"2025-05-21T15:03:33+02:00\",\"created_at\":\"2025-05-21T15:03:33+02:00\",\"type\":\"SDI\",\"complete\":true,\"main\":true,\"encrypted\":false,\"is_radio\":false,\"eit\":false,\"eit_pf\":false,\"transparent_mpts\":false,\"service_type\":25,\"service_type_label\":\"HDTV (0x19)\",\"tenant_ids\":[1],\"service_provider\":[],\"usage_type\":\"ALL\",\"sid_1\":null,\"sid_2\":null,\"sid_3\":null,\"sid_4\":null,\"source_asi\":null,\"source_fm\":null,\"source_dvbt2\":null,\"source_ip\":null,\"source_sat\":null,\"source_sdi\":{\"guid\":\"88879149e783e554601e2089f06165085bb206cc\",\"updated_at\":\"2025-05-21T15:03:33+02:00\",\"created_at\":\"2025-05-21T15:03:33+02:00\",\"standard_1\":\"HD-SDI\",\"physical_port_1\":\"7.2B\",\"location_1\":{\"id\":1012,\"region\":\"OTHER\",\"name\":\"Potsdam A, Marlene-Dietrich-Allee 20, 14882 Potsdam, Raum Sendezentrum Fernsehen (SZF), Hybnet-Raum \",\"loc_type\":4,\"loc_type_label\":\"Foreign site\",\"host_prefix\":\"p-marl-x20-ard-1\",\"address\":null,\"room\":null,\"postcode\":null,\"city\":null},\"standard_2\":\"HD-SDI\",\"physical_port_2\":\"7.2B\",\"location_2\":{\"id\":1034,\"region\":\"OTHER\",\"name\":\"Potsdam B, Marlene-Dietrich-Allee 20, 14882 Potsdam, Raum Radiohaus (RDH), ZGR, p-marl-x20-ard-2\",\"loc_type\":4,\"loc_type_label\":\"Foreign site\",\"host_prefix\":\"p-marl-x20-ard-2\",\"address\":null,\"room\":null,\"postcode\":null,\"city\":null},\"standard_3\":null,\"physical_port_3\":null,\"location_3\":null,\"standard_4\":null,\"physical_port_4\":null,\"location_4\":null},\"source_srt\":null,\"source_stream\":null,\"programs\":[434,7895],\"additional_information\":null,\"pids\":{\"video\":null,\"pmt\":null,\"pcr\":null,\"teletext\":null,\"subtitle\":null,\"ait_hbbtv\":null,\"eit\":null,\"sdt\":null,\"audio\":[]},\"notice\":\"SDI Pre-Enc\"},\"readyForProvision\":false,\"usageType\":\"ALL\",\"createdAt\":\"2025-05-21T14:45:35.633Z\",\"updatedAt\":\"2025-05-21T14:45:35.633Z\",\"programs\":[]}";
		private static readonly string AcquisitionData4 =
			"{\"id\":\"VFDE:4393\",\"referenceProgramIds\":[8261,8285],\"name\":\"RTL_Bayern_Nürnberg_SI/IP/1\",\"tenant\":\"VFDE\",\"sourceId\":\"d140a8cf9e51bd87f4ac54fb175e47b959886e9a\",\"sourceType\":\"IP\",\"details\":{\"usecase\":\"[SRM] IP Peering Channels Dual Source\",\"_id\":\"682de76f4ff8c7c10f90b091\",\"updated_at\":\"2025-05-21T15:05:09+02:00\",\"created_at\":\"2025-05-21T15:03:44+02:00\",\"type\":\"IP\",\"complete\":true,\"main\":true,\"encrypted\":false,\"is_radio\":false,\"eit\":true,\"eit_pf\":true,\"transparent_mpts\":false,\"service_type\":1,\"service_type_label\":\"TV (0x01)\",\"tenant_ids\":[1],\"service_provider\":[193],\"usage_type\":\"SI\",\"sid_1\":51063,\"sid_2\":51063,\"sid_3\":51063,\"sid_4\":51063,\"source_asi\":null,\"source_fm\":null,\"source_dvbt2\":null,\"source_ip\":{\"guid\":\"d140a8cf9e51bd87f4ac54fb175e47b959886e9a\",\"updated_at\":\"2025-05-21T15:05:07+02:00\",\"created_at\":\"2025-05-21T15:03:43+02:00\",\"multicast_ip_1a\":\"234.168.7.1:60000\",\"source_ip_1a\":\"10.97.168.7\",\"nid_1a\":61441,\"tid_1a\":10000,\"sid_1a\":51063,\"location_1a\":{\"id\":989,\"region\":\"OTHER\",\"name\":\"PoC Kerpen\",\"loc_type\":4,\"loc_type_label\":\"Foreign site\",\"host_prefix\":\"bm-mich\",\"address\":null,\"room\":null,\"postcode\":null,\"city\":null},\"multicast_ip_1b\":\"234.168.7.1:60000\",\"source_ip_1b\":\"10.97.168.7\",\"nid_1b\":61441,\"tid_1b\":10000,\"sid_1b\":null,\"location_1b\":{\"id\":1024,\"region\":\"OTHER\",\"name\":\"PoC Rödelheim\",\"loc_type\":4,\"loc_type_label\":\"Foreign site\",\"host_prefix\":\"f-brei\",\"address\":null,\"room\":null,\"postcode\":null,\"city\":null},\"multicast_ip_2a\":\"234.142.1.9:60000\",\"source_ip_2a\":\"10.97.142.1\",\"nid_2a\":61441,\"tid_2a\":10000,\"sid_2a\":51063,\"location_2a\":{\"id\":989,\"region\":\"OTHER\",\"name\":\"PoC Kerpen\",\"loc_type\":4,\"loc_type_label\":\"Foreign site\",\"host_prefix\":\"bm-mich\",\"address\":null,\"room\":null,\"postcode\":null,\"city\":null},\"multicast_ip_2b\":\"234.142.1.9:60000\",\"source_ip_2b\":\"10.97.142.1\",\"nid_2b\":61441,\"tid_2b\":10000,\"sid_2b\":51063,\"location_2b\":{\"id\":1024,\"region\":\"OTHER\",\"name\":\"PoC Rödelheim\",\"loc_type\":4,\"loc_type_label\":\"Foreign site\",\"host_prefix\":\"f-brei\",\"address\":null,\"room\":null,\"postcode\":null,\"city\":null}},\"source_sat\":null,\"source_sdi\":null,\"source_srt\":null,\"source_stream\":null,\"programs\":[8261,8285],\"additional_information\":null,\"pids\":{\"video\":null,\"pmt\":null,\"pcr\":null,\"teletext\":null,\"subtitle\":null,\"ait_hbbtv\":null,\"eit\":3318,\"sdt\":3317,\"audio\":[]},\"notice\":\"Migration RTL_Bayern_Nürnberg_SI/IP/1 + RTL_Bayern_Nürnberg_SI/IP/2\"},\"readyForProvision\":true,\"usageType\":\"SI\",\"createdAt\":\"2025-05-21T14:47:12.024Z\",\"updatedAt\":\"2025-06-10T10:55:42.864Z\",\"complete\":{\"readyForUse\":true,\"doneAt\":\"2025-06-10T10:55:42.864Z\",\"user\":{\"id\":\"6458bba7cad2c7a9893f153c\",\"name\":\"LUPO internal\"},\"_id\":\"68480f2eeb938922b6880da7\"},\"dataMinerResponse\":{\"dataMinerServiceId\":\"39118/6482\",\"provisionedAt\":\"2024-07-29T10:20:39.372Z\",\"message\":\"Initial provisioning based on AggregationID import\",\"_id\":\"68480f2eeb938922b6880da5\"},\"deploymentAt\":\"2024-07-29T10:20:39.372Z\",\"noticedAt\":\"2024-07-29T10:20:39.372Z\",\"testResult\":{\"result\":\"accepted\",\"doneAt\":\"2025-06-10T10:55:42.864Z\",\"comment\":\"AUTOMATICALLY ACCEPTED\",\"user\":{\"id\":\"6458bba7cad2c7a9893f153c\",\"name\":\"LUPO internal\"},\"_id\":\"68480f2eeb938922b6880da6\"},\"programs\":[]}";

		private Action<string> _logger;
		private static string NameFrequency = "Frequency";
		private static string NameSymbolRate = "Symbol Rate";
		private static string NameDeviceMain = "Device Main";
		private static string PropertyNameTenant;

		/// <summary>
		///     The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			try
			{
				_logger = engine.GenerateInformation;
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

		private static List<Models.ServiceSpecificationConfigurationValue> BuildConfigsSpec(LupoAcquisitionData.Root model, List<Models.ServiceSpecificationConfigurationValue> existing)
		{
			switch (model.Details.Type)
			{
				case "SAT":
					return BuildConfigsSatDl(model, existing);

				case "Stream":
					return BuildConfigsRadio(model, existing);

				case "SDI":
					return BuildConfigsSdiPenc(model, existing);

				case "IP":
					return BuildConfigPeering(model, existing);

				default:
					throw new NotImplementedException($"Type '{model.Details.Type}' not implemented (yet).");
			}
		}

		private static List<Models.ServiceOrderItemConfigurationValue> BuildConfigsOrder(LupoAcquisitionData.Root model, List<Models.ServiceOrderItemConfigurationValue> existing, Models.ServiceSpecification spec)
		{
			switch (model.Details.Type)
			{
				case "SAT":
					return BuildConfigsServiceOrderSatDl(model, existing, spec);

				case "Stream":
					return BuildConfigsServiceOrderRadio(model, existing, spec);

				case "SDI":
					return BuildConfigsServiceOrderSdiPenc(model, existing, spec);

				case "IP":
					return BuildConfigsServiceOrderPeering(model, existing, spec);

				default:
					throw new NotImplementedException($"Type '{model.Details.Type}' not implemented (yet).");
			}
		}

		private static List<Models.ServiceSpecificationConfigurationValue> BuildConfigsSatDl(LupoAcquisitionData.Root model, List<Models.ServiceSpecificationConfigurationValue> existing)
		{
			var dataHelper = new DataHelperConfigurationParameter(Engine.SLNetRaw);
			var existingConfigParams = dataHelper.Read();
			var paramPolarization = existingConfigParams.Find(x => x.Name == NamePolarization) ?? CreateParamPolarization(dataHelper, NamePolarization);
			var paramOrbital = existingConfigParams.Find(x => x.Name == NameOrbitalPosition) ?? CreateParamOrbitalPosition(dataHelper, NameOrbitalPosition);
			var paramFreq = existingConfigParams.Find(x => x.Name == NameFrequency) ?? CreateParamFrequency(dataHelper, NameFrequency);
			var paramSymbolRate = existingConfigParams.Find(x => x.Name == NameSymbolRate) ?? CreateParamSymbolRate(dataHelper, NameSymbolRate);
			var paramSid = existingConfigParams.Find(x => x.Name == NameServiceId) ?? CreateParamServiceId(dataHelper, NameServiceId);
			var paramTransponder = existingConfigParams.Find(x => x.Name == NameTransponder) ?? CreateParameterString(dataHelper, NameTransponder);

			var dataHelperValue = new DataHelperConfigurationParameterValue(Engine.SLNetRaw);
			var orbitalOptions = paramOrbital.DiscreteOptions.DiscreteValues.Select(x => x.Value).ToList();
			string orbital = orbitalOptions.First(x => x.StartsWith(model.Details.SourceSat.OrbitalPosition.Replace(",", ".").Split('°')[0]));

			return new List<Models.ServiceSpecificationConfigurationValue>
			{
				new Models.ServiceSpecificationConfigurationValue
				{
					ID = existing.Find(x => x.ConfigurationParameter.Label == NamePolarization)?.ID ?? Guid.NewGuid(),
					MandatoryAtService = true,
					MandatoryAtServiceOrder = true,
					ExposeAtServiceOrder = true,
					ConfigurationParameter = CreateConfig(dataHelperValue, paramPolarization, NamePolarization, model.Details.SourceSat.Polarisation.ToUpper().StartsWith("V") ? "Vertical" : "Horizontal"),
				},
				new Models.ServiceSpecificationConfigurationValue
				{
					ID = existing.Find(x => x.ConfigurationParameter.Label == NameOrbitalPosition)?.ID ?? Guid.NewGuid(),
					MandatoryAtService = true,
					MandatoryAtServiceOrder = true,
					ExposeAtServiceOrder = true,
					ConfigurationParameter = CreateConfig(dataHelperValue, paramOrbital, NameOrbitalPosition, orbital),
				},
				new Models.ServiceSpecificationConfigurationValue
				{
					ID = existing.Find(x => x.ConfigurationParameter.Label == NameFrequency)?.ID ?? Guid.NewGuid(),
					MandatoryAtService = true,
					MandatoryAtServiceOrder = true,
					ExposeAtServiceOrder = true,
					ConfigurationParameter = CreateConfig(dataHelperValue, paramFreq, NameFrequency, model.Details.SourceSat.Frequency / 1_000.0),
				},
				new Models.ServiceSpecificationConfigurationValue
				{
					ID = existing.Find(x => x.ConfigurationParameter.Label == NameSymbolRate)?.ID ?? Guid.NewGuid(),
					MandatoryAtService = true,
					MandatoryAtServiceOrder = true,
					ExposeAtServiceOrder = true,
					ConfigurationParameter = CreateConfig(dataHelperValue, paramSymbolRate, NameSymbolRate, model.Details.SourceSat.SymbolRate / 1_000.0),
				},
				new Models.ServiceSpecificationConfigurationValue
				{
					ID = existing.Find(x => x.ConfigurationParameter.Label == NameServiceId)?.ID ?? Guid.NewGuid(),
					MandatoryAtService = true,
					MandatoryAtServiceOrder = true,
					ExposeAtServiceOrder = true,
					ConfigurationParameter = CreateConfig(dataHelperValue, paramSid, NameServiceId, model.Details.SourceSat.Sid),
				},
				new Models.ServiceSpecificationConfigurationValue
				{
					ID = existing.Find(x => x.ConfigurationParameter.Label == NameTransponder)?.ID ?? Guid.NewGuid(),
					MandatoryAtService = true,
					MandatoryAtServiceOrder = true,
					ExposeAtServiceOrder = true,
					ConfigurationParameter = CreateConfig(dataHelperValue, paramTransponder, NameTransponder, model.Details.SourceSat.Transponder),
				},
			};
		}

		private static List<Models.ServiceOrderItemConfigurationValue> BuildConfigsServiceOrderSatDl(LupoAcquisitionData.Root model, List<Models.ServiceOrderItemConfigurationValue> existing, Models.ServiceSpecification spec)
		{
			var dataHelper = new DataHelperConfigurationParameter(Engine.SLNetRaw);
			var existingConfigParams = dataHelper.Read();
			var paramOrbital = existingConfigParams.Find(x => x.Name == NameOrbitalPosition) ?? CreateParamOrbitalPosition(dataHelper, NameOrbitalPosition);
			var orbitalOptions = paramOrbital.DiscreteOptions.DiscreteValues.Select(x => x.Value).ToList();
			string orbitalVal = orbitalOptions.First(x => x.StartsWith(model.Details.SourceSat.OrbitalPosition.Split(',')[0]));

			var polarization = GetServiceOrderItemConfigurationValue(existing, spec, NamePolarization);
			var orbital = GetServiceOrderItemConfigurationValue(existing, spec, NameOrbitalPosition);
			var freq = GetServiceOrderItemConfigurationValue(existing, spec, NameFrequency);
			var symbRate = GetServiceOrderItemConfigurationValue(existing, spec, NameSymbolRate);
			var sid = GetServiceOrderItemConfigurationValue(existing, spec, NameServiceId);
			var transponder = GetServiceOrderItemConfigurationValue(existing, spec, NameTransponder);

			polarization.ConfigurationParameter.StringValue = model.Details.SourceSat.Polarisation.ToUpper().StartsWith("V") ? "Vertical" : "Horizontal";
			orbital.ConfigurationParameter.StringValue = orbitalVal;
			freq.ConfigurationParameter.DoubleValue = model.Details.SourceSat.Frequency / 1_000.0;
			symbRate.ConfigurationParameter.DoubleValue = model.Details.SourceSat.SymbolRate / 1_000.0;
			sid.ConfigurationParameter.DoubleValue = model.Details.Programs.FirstOrDefault();
			transponder.ConfigurationParameter.StringValue = model.Details.SourceSat.Transponder;

			return new List<Models.ServiceOrderItemConfigurationValue>
			{
				polarization,
				orbital,
				freq,
				symbRate,
				sid,
				transponder,
			};
		}

		private static List<Models.ServiceSpecificationConfigurationValue> BuildConfigsRadio(LupoAcquisitionData.Root model, List<Models.ServiceSpecificationConfigurationValue> existing)
		{
			var dataHelper = new DataHelperConfigurationParameter(Engine.SLNetRaw);
			var existingParams = dataHelper.Read();
			var paramSid = existingParams.Find(x => x.Name == NameServiceId) ?? CreateParamServiceId(dataHelper, NameServiceId);
			var paramMainUrl = existingParams.Find(x => x.Name == NameUrl) ?? CreateParameterString(dataHelper, NameUrl);

			var dataHelperValue = new DataHelperConfigurationParameterValue(Engine.SLNetRaw);

			return new List<Models.ServiceSpecificationConfigurationValue>
			{
				new Models.ServiceSpecificationConfigurationValue
				{
					ID = existing.Find(x => x.ConfigurationParameter.Label == NameServiceId)?.ID ?? Guid.NewGuid(),
					MandatoryAtService = true,
					MandatoryAtServiceOrder = true,
					ExposeAtServiceOrder = true,
					ConfigurationParameter = CreateConfig(dataHelperValue, paramSid, NameServiceId, model.Details.Programs.FirstOrDefault()),
				},
				new Models.ServiceSpecificationConfigurationValue
				{
					ID = existing.Find(x => x.ConfigurationParameter.Label == NameUrlMain)?.ID ?? Guid.NewGuid(),
					MandatoryAtService = true,
					MandatoryAtServiceOrder = true,
					ExposeAtServiceOrder = true,
					ConfigurationParameter = CreateConfig(dataHelperValue, paramMainUrl, NameUrlMain, model.Details.SourceStream.Url1A?.ToString() ?? String.Empty),
				},
				new Models.ServiceSpecificationConfigurationValue
				{
					ID = existing.Find(x => x.ConfigurationParameter.Label == NameUrlBackup)?.ID ?? Guid.NewGuid(),
					MandatoryAtService = true,
					MandatoryAtServiceOrder = true,
					ExposeAtServiceOrder = true,
					ConfigurationParameter = CreateConfig(dataHelperValue, paramMainUrl, NameUrlBackup, model.Details.SourceStream.Url1B?.ToString() ?? String.Empty),
				},
			};
		}

		private static List<Models.ServiceOrderItemConfigurationValue> BuildConfigsServiceOrderRadio(LupoAcquisitionData.Root model, List<Models.ServiceOrderItemConfigurationValue> existing, Models.ServiceSpecification spec)
		{
			var sid = GetServiceOrderItemConfigurationValue(existing, spec, NameServiceId);
			var urlMain = GetServiceOrderItemConfigurationValue(existing, spec, NameUrlMain);
			var urlBkup = GetServiceOrderItemConfigurationValue(existing, spec, NameUrlBackup);

			sid.ConfigurationParameter.DoubleValue = model.Details.Programs.FirstOrDefault();
			urlMain.ConfigurationParameter.StringValue = model.Details.SourceStream.Url1A?.ToString() ?? String.Empty;
			urlBkup.ConfigurationParameter.StringValue = model.Details.SourceStream.Url1B?.ToString() ?? String.Empty;

			return new List<Models.ServiceOrderItemConfigurationValue>
			{
				sid,
				urlMain,
				urlBkup,
			};
		}

		private static List<Models.ServiceSpecificationConfigurationValue> BuildConfigPeering(LupoAcquisitionData.Root model, List<Models.ServiceSpecificationConfigurationValue> existing)
		{
			var dataHelper = new DataHelperConfigurationParameter(Engine.SLNetRaw);
			var existingParams = dataHelper.Read();
			var paramSid = existingParams.Find(x => x.Name == NameServiceId) ?? CreateParamServiceId(dataHelper, NameServiceId);
			var paramAddress = existingParams.Find(x => x.Name == NameMulticast) ?? CreateParameterString(dataHelper, NameMulticast);
			var paramSourceIp = existingParams.Find(x => x.Name == NameSourceIp) ?? CreateParameterString(dataHelper, NameSourceIp);

			var dataHelperValue = new DataHelperConfigurationParameterValue(Engine.SLNetRaw);

			return new List<Models.ServiceSpecificationConfigurationValue>
			{
				new Models.ServiceSpecificationConfigurationValue
				{
					ID = existing.Find(x => x.ConfigurationParameter.Label == NameServiceId)?.ID ?? Guid.NewGuid(),
					MandatoryAtService = true,
					MandatoryAtServiceOrder = true,
					ExposeAtServiceOrder = true,
					ConfigurationParameter = CreateConfig(dataHelperValue, paramSid, NameServiceId, model.Details.Programs.FirstOrDefault()),
				},
				new Models.ServiceSpecificationConfigurationValue
				{
					ID = existing.Find(x => x.ConfigurationParameter.Label == NameMulticastMain)?.ID ?? Guid.NewGuid(),
					MandatoryAtService = true,
					MandatoryAtServiceOrder = true,
					ExposeAtServiceOrder = true,
					ConfigurationParameter = CreateConfig(dataHelperValue, paramAddress, NameMulticastMain, model.Details.SourceIp.MulticastIp1A ?? String.Empty),
				},
				new Models.ServiceSpecificationConfigurationValue
				{
					ID = existing.Find(x => x.ConfigurationParameter.Label == NameMulticastBackup)?.ID ?? Guid.NewGuid(),
					MandatoryAtService = true,
					MandatoryAtServiceOrder = true,
					ExposeAtServiceOrder = true,
					ConfigurationParameter = CreateConfig(dataHelperValue, paramAddress, NameMulticastBackup, model.Details.SourceIp.MulticastIp2A ?? String.Empty),
				},
				new Models.ServiceSpecificationConfigurationValue
				{
					ID = existing.Find(x => x.ConfigurationParameter.Label == NameSourceIpMain)?.ID ?? Guid.NewGuid(),
					MandatoryAtService = true,
					MandatoryAtServiceOrder = true,
					ExposeAtServiceOrder = true,
					ConfigurationParameter = CreateConfig(dataHelperValue, paramSourceIp, NameSourceIpMain, model.Details.SourceIp.SourceIp1A ?? String.Empty),
				},
				new Models.ServiceSpecificationConfigurationValue
				{
					ID = existing.Find(x => x.ConfigurationParameter.Label == NameSourceIpBackup)?.ID ?? Guid.NewGuid(),
					MandatoryAtService = true,
					MandatoryAtServiceOrder = true,
					ExposeAtServiceOrder = true,
					ConfigurationParameter = CreateConfig(dataHelperValue, paramSourceIp, NameSourceIpBackup, model.Details.SourceIp.SourceIp2A ?? String.Empty),
				},
			};
		}

		private static List<Models.ServiceOrderItemConfigurationValue> BuildConfigsServiceOrderPeering(LupoAcquisitionData.Root model, List<Models.ServiceOrderItemConfigurationValue> existing, Models.ServiceSpecification spec)
		{
			var sid = GetServiceOrderItemConfigurationValue(existing, spec, NameServiceId);
			var addressMain = GetServiceOrderItemConfigurationValue(existing, spec, NameMulticastMain);
			var addressBkup = GetServiceOrderItemConfigurationValue(existing, spec, NameMulticastBackup);
			var sourceMain = GetServiceOrderItemConfigurationValue(existing, spec, NameSourceIpMain);
			var sourceBkup = GetServiceOrderItemConfigurationValue(existing, spec, NameSourceIpBackup);

			sid.ConfigurationParameter.DoubleValue = model.Details.Programs.FirstOrDefault();
			addressMain.ConfigurationParameter.StringValue = model.Details.SourceIp.MulticastIp1A ?? String.Empty;
			addressBkup.ConfigurationParameter.StringValue = model.Details.SourceIp.MulticastIp2A ?? String.Empty;
			sourceMain.ConfigurationParameter.StringValue = model.Details.SourceIp.SourceIp1A ?? String.Empty;
			sourceBkup.ConfigurationParameter.StringValue = model.Details.SourceIp.SourceIp2A ?? String.Empty;

			return new List<Models.ServiceOrderItemConfigurationValue>
			{
				sid,
				addressMain,
				addressBkup,
				sourceMain,
				sourceBkup,
			};
		}

		private static List<Models.ServiceSpecificationConfigurationValue> BuildConfigsSdiPenc(LupoAcquisitionData.Root model, List<Models.ServiceSpecificationConfigurationValue> existing)
		{
			var dataHelper = new DataHelperConfigurationParameter(Engine.SLNetRaw);
			var existingConfigParams = dataHelper.Read();
			var paramIntf = existingConfigParams.Find(x => x.Name == NameInterface) ?? CreateParameterString(dataHelper, NameInterface);
			var paramDevice = existingConfigParams.Find(x => x.Name == NameDevice) ?? CreateParameterString(dataHelper, NameDevice);

			var dataHelperValue = new DataHelperConfigurationParameterValue(Engine.SLNetRaw);

			return new List<Models.ServiceSpecificationConfigurationValue>
			{
				new Models.ServiceSpecificationConfigurationValue
				{
					ID = existing.Find(x => x.ConfigurationParameter.Label == NameInterfaceMain)?.ID ?? Guid.NewGuid(),
					MandatoryAtService = true,
					MandatoryAtServiceOrder = true,
					ExposeAtServiceOrder = true,
					ConfigurationParameter = CreateConfig(dataHelperValue, paramIntf, NameInterfaceMain, model.Details.SourceSdi.PhysicalPort1 ?? String.Empty),
				},
				new Models.ServiceSpecificationConfigurationValue
				{
					ID = existing.Find(x => x.ConfigurationParameter.Label == NameInterfaceBackup)?.ID ?? Guid.NewGuid(),
					MandatoryAtServiceOrder = true,
					MandatoryAtService = true,
					ExposeAtServiceOrder = true,
					ConfigurationParameter = CreateConfig(dataHelperValue, paramIntf, NameInterfaceBackup, model.Details.SourceSdi.PhysicalPort2 ?? String.Empty),
				},
				new Models.ServiceSpecificationConfigurationValue
				{
					ID = existing.Find(x => x.ConfigurationParameter.Label == NameDeviceMain)?.ID ?? Guid.NewGuid(),
					MandatoryAtServiceOrder = true,
					MandatoryAtService = true,
					ExposeAtServiceOrder = true,
					ConfigurationParameter = CreateConfig(dataHelperValue, paramDevice, NameDeviceMain, model.Details.SourceSdi.Location1?.HostPrefix ?? String.Empty),
				},
				new Models.ServiceSpecificationConfigurationValue
				{
					ID = existing.Find(x => x.ConfigurationParameter.Label == NameDeviceBackup)?.ID ?? Guid.NewGuid(),
					MandatoryAtServiceOrder = true,
					MandatoryAtService = true,
					ExposeAtServiceOrder = true,
					ConfigurationParameter = CreateConfig(dataHelperValue, paramDevice, NameDeviceBackup, model.Details.SourceSdi.Location2?.HostPrefix ?? String.Empty),
				},
			};
		}

		private static List<Models.ServiceOrderItemConfigurationValue> BuildConfigsServiceOrderSdiPenc(LupoAcquisitionData.Root model, List<Models.ServiceOrderItemConfigurationValue> existing, Models.ServiceSpecification spec)
		{
			var intfMain = GetServiceOrderItemConfigurationValue(existing, spec, NameInterfaceMain);
			var intfBkup = GetServiceOrderItemConfigurationValue(existing, spec, NameInterfaceBackup);
			var deviceMain = GetServiceOrderItemConfigurationValue(existing, spec, NameDeviceMain);
			var deviceBkup = GetServiceOrderItemConfigurationValue(existing, spec, NameDeviceBackup);

			intfMain.ConfigurationParameter.StringValue = model.Details.SourceSdi.PhysicalPort1 ?? String.Empty;
			intfBkup.ConfigurationParameter.StringValue = model.Details.SourceSdi.PhysicalPort2 ?? String.Empty;
			deviceMain.ConfigurationParameter.StringValue = model.Details.SourceSdi.Location1?.HostPrefix ?? String.Empty;
			deviceBkup.ConfigurationParameter.StringValue = model.Details.SourceSdi.Location2?.HostPrefix ?? String.Empty;

			return new List<Models.ServiceOrderItemConfigurationValue>
			{
				intfMain,
				intfBkup,
				deviceMain,
				deviceBkup,
			};
		}

		private static Models.ServiceOrderItemConfigurationValue GetServiceOrderItemConfigurationValue(List<Models.ServiceOrderItemConfigurationValue> existing, Models.ServiceSpecification spec, string parameterLabel)
		{
			var intfMain = existing.Find(x => x.ConfigurationParameter?.Label == parameterLabel);
			if (intfMain == null)
			{
				var cf = spec.Configurations.Find(x => x.ConfigurationParameter?.Label == parameterLabel)
						 ?? throw new InvalidOperationException($"Specification does not contain the expected '{parameterLabel}'");
				intfMain = new Models.ServiceOrderItemConfigurationValue
				{
					ID = Guid.NewGuid(),
					Mandatory = cf.MandatoryAtServiceOrder,
					ConfigurationParameter = cf.ConfigurationParameter ?? throw new InvalidOperationException($"Configuration '{cf.ID}' has no parameter linked!"),
				};
				intfMain.ConfigurationParameter.ID = Guid.NewGuid();
			}

			return intfMain;
		}

		private static Models.ServicePropertyValues BuildProperties(LupoAcquisitionData.Root model)
		{
			var dataHelper = new DataHelperServiceProperties(Engine.SLNetRaw);
			var existingProperties = dataHelper.Read();

			PropertyNameTenant = "Tenant";
			var propTenantId = existingProperties.Find(x => x.Name == PropertyNameTenant)?.ID ?? CreateProperty(dataHelper, PropertyNameTenant);

			return new Models.ServicePropertyValues
			{
				ID = Guid.NewGuid(),
				Values = new List<Models.ServicePropertyValue>
				{
					new Models.ServicePropertyValue
					{
						ServicePropertyId = propTenantId,
						Value = model.Tenant,
					},
				},
			};
		}

		private static Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter CreateParamFrequency(DataHelperConfigurationParameter helper, string name)
		{
			var id = helper.CreateOrUpdate(
				new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter
				{
					Name = name,
					Type = SlcConfigurationsIds.Enums.Type.Number,
					NumberOptions = new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.NumberParameterOptions
					{
						Decimals = 6,
						MinRange = 0,
						StepSize = 0.000001,
						DefaultUnit = new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationUnit { Name = "MHz" },
						Units = new List<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationUnit>
						{
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationUnit { Name = "MHz" },
						},
					},
				});
			return helper.Read().First(x => x.ID == id);
		}

		private static Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter CreateParamOrbitalPosition(DataHelperConfigurationParameter helper, string name)
		{
			var id = helper.CreateOrUpdate(
				new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter
				{
					Name = name,
					Type = SlcConfigurationsIds.Enums.Type.Discrete,
					DiscreteOptions = new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteParameterOptions
					{
						Default = new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "19.2E" },
						DiscreteValues = new List<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue>
						{
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "0.8W" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "3.0E" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "4.8E" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "5.0W" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "7.0E" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "9.0E" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "12.5W" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "13.0E" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "15.0W" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "16.0E" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "19.2E" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "21.5E" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "23.5E" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "26.0E" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "28.2E" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "30.5E" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "33.0E" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "34.5W" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "36.0E" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "39.0E" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "42.0E" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "48.0E" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "55.0E" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "68.5E" },
						},
					},
				});
			return helper.Read().First(x => x.ID == id);
		}

		private static Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter CreateParamPolarization(DataHelperConfigurationParameter helper, string name)
		{
			var id = helper.CreateOrUpdate(
				new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter
				{
					Name = name,
					Type = SlcConfigurationsIds.Enums.Type.Discrete,
					DiscreteOptions = new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteParameterOptions
					{
						Default = new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "Horizontal" },
						DiscreteValues = new List<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue>
						{
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "Horizontal" },
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue { Value = "Vertical" },
						},
					},
				});
			return helper.Read().First(x => x.ID == id);
		}

		private static Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter CreateParamServiceId(DataHelperConfigurationParameter helper, string name)
		{
			var id = helper.CreateOrUpdate(
				new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter
				{
					Name = name,
					Type = SlcConfigurationsIds.Enums.Type.Number,
					NumberOptions = new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.NumberParameterOptions
					{
						MinRange = 0,
						MaxRange = 50000,
						StepSize = 1,
						Decimals = 0,
					},
				});
			return helper.Read().First(x => x.ID == id);
		}

		private static Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter CreateParameterString(DataHelperConfigurationParameter helper, string name)
		{
			var id = helper.CreateOrUpdate(
				new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter
				{
					Name = name,
					Type = SlcConfigurationsIds.Enums.Type.Text,
				});
			return helper.Read().First(x => x.ID == id);
		}

		private static Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameterValue CreateConfig(DataHelperConfigurationParameterValue helper, Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter parameter, string name, string value)
		{
			var id = helper.CreateOrUpdate(
				new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameterValue
				{
					Label = name,
					Type = parameter.Type,
					ConfigurationParameterId = parameter.ID,
					StringValue = value,
					DiscreteOptions = parameter.DiscreteOptions,
					NumberOptions = parameter.NumberOptions,
					TextOptions = parameter.TextOptions,
				});
			return helper.Read().First(x => x.ID == id);
		}

		private static Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameterValue CreateConfig(DataHelperConfigurationParameterValue helper, Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter parameter, string name, double value)
		{
			var id = helper.CreateOrUpdate(
				new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameterValue
				{
					Label = name,
					Type = parameter.Type,
					ConfigurationParameterId = parameter.ID,
					DoubleValue = value,
					DiscreteOptions = parameter.DiscreteOptions,
					NumberOptions = parameter.NumberOptions,
					TextOptions = parameter.TextOptions,
				});
			return helper.Read().First(x => x.ID == id);
		}

		private static Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter CreateParamSymbolRate(DataHelperConfigurationParameter helper, string name)
		{
			var id = helper.CreateOrUpdate(
				new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter
				{
					Name = name,
					Type = SlcConfigurationsIds.Enums.Type.Number,
					NumberOptions = new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.NumberParameterOptions
					{
						Decimals = 6,
						MinRange = 0,
						StepSize = 0.000001,
						DefaultUnit = new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationUnit { Name = "MBd" },
						Units = new List<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationUnit>
						{
							new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationUnit { Name = "MBd" },
						},
					},
				});
			return helper.Read().First(x => x.ID == id);
		}

		private static Guid CreateProperty(DataHelperServiceProperties helper, string name)
		{
			return helper.CreateOrUpdate(
				new Models.ServiceProperty
				{
					Name = name,
					Type = SlcServicemanagementIds.Enums.TypeEnum.String,
					DiscreteValues = new List<string>(),
				});
		}

		private static Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization.Models.Organization GetOrganization()
		{
			var dataHelperOrg = new Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization.DataHelperOrganization(Engine.SLNetRaw);
			var org = dataHelperOrg.Read().Find(x => x.Name == "Vodafone Kabel Deutschland");
			if (org == null)
			{
				var dataHelperCat = new Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization.DataHelperCategory(Engine.SLNetRaw);
				var category = dataHelperCat.Read().Find(x => x.Name == "Content Provider");
				if (category == null)
				{
					category = new Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization.Models.Category
					{
						ID = Guid.NewGuid(),
						Name = "Content Provider",
					};
					dataHelperCat.CreateOrUpdate(category);
				}

				org = new Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization.Models.Organization
				{
					ID = Guid.NewGuid(),
					Name = "Vodafone Kabel Deutschland",
					CategoryId = category.ID,
				};
				dataHelperOrg.CreateOrUpdate(org);
			}

			return org;
		}

		private static Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization.Models.People GetPeople(Guid org)
		{
			var dataHelperPeople = new DataHelperPeople(Engine.SLNetRaw);
			var people = dataHelperPeople.Read().Find(x => x.OrganizationId == org);
			if (people == null)
			{
				var dataHelperExperience = new DataHelperPersonalExperience(Engine.SLNetRaw);
				var exp = dataHelperExperience.Read().Find(x => x.Value == "Master");
				if (exp == null)
				{
					exp = new Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization.Models.ExperienceLevel
					{
						ID = Guid.NewGuid(),
						Value = "Master",
					};
					dataHelperExperience.CreateOrUpdate(exp);
				}

				people = new Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization.Models.People
				{
					ID = Guid.NewGuid(),
					OrganizationId = org,
					FullName = "Ops 1",
					Mail = "ops1@mail.com",
					Phone = "+32494/00.00.00",
					Skill = "Operator",
					ExperienceLevel = exp,
				};
				dataHelperPeople.CreateOrUpdate(people);
			}

			return people;
		}

		private void ImportAcquisitionDataAsServiceOrder(LupoAcquisitionData.Root model)
		{
			var dataHelperServiceSpecification = new DataHelperServiceSpecification(Engine.SLNetRaw);
			var specification = dataHelperServiceSpecification.Read().Find(x => x.Name == model.Details.Type);
			if (specification == null)
			{
				throw new ArgumentNullException($"A Service Specification doesn't exist yet for type '{model.Details.Type}'");
			}

			var dataHelperServiceOrder = new DataHelperServiceOrder(Engine.SLNetRaw);

			var org = GetOrganization();
			var contact = GetPeople(org.ID);

			var serviceOrder = dataHelperServiceOrder.Read().Find(x => x.ExternalID == model.Id)
							   ?? new Models.ServiceOrder { ID = Guid.NewGuid(), ExternalID = model.Id };

			var orderItemName = $"{model.Name} [{model.Details.Type}]";
			Models.ServicePropertyValues prop = BuildProperties(model);

			var existingConfigs = new List<Models.ServiceOrderItemConfigurationValue>();
			var orderItem = serviceOrder.OrderItems?.Find(o => o.ServiceOrderItem?.Name == orderItemName);
			if (orderItem != null)
			{
				if (orderItem.ServiceOrderItem.Properties != null)
				{
					// re-use existing properties to avoid creating new instances each time
					prop.ID = orderItem.ServiceOrderItem.Properties.ID;
				}

				if (orderItem.ServiceOrderItem.Configurations != null)
				{
					existingConfigs = orderItem.ServiceOrderItem.Configurations;
				}
			}

			var configs = BuildConfigsOrder(model, existingConfigs, specification);

			serviceOrder.Name = model.Name;
			serviceOrder.Description = model.Name;
			serviceOrder.Priority = SlcServicemanagementIds.Enums.ServiceorderpriorityEnum.Medium;
			serviceOrder.OrganizationId = org.ID;
			serviceOrder.ContactIds = new List<Guid> { contact.ID };
			var soi = new Models.ServiceOrderItems
			{
				Priority = 1,
				ServiceOrderItem = new Models.ServiceOrderItem
				{
					ID = orderItem?.ServiceOrderItem.ID ?? Guid.NewGuid(),
					StatusId = orderItem?.ServiceOrderItem?.StatusId,
					Action = "Add",
					Name = orderItemName,
					StartTime = model.DeploymentAt != null ? DateTime.Parse(model.DeploymentAt) : DateTime.UtcNow + TimeSpan.FromDays(7),
					EndTime = default,
					IndefiniteRuntime = true,
					ServiceCategoryId = orderItem?.ServiceOrderItem.ServiceCategoryId,
					Properties = prop,
					Configurations = configs,
					ServiceId = orderItem?.ServiceOrderItem.ServiceId,
					SpecificationId = specification.ID,
				},
			};
			serviceOrder.OrderItems = serviceOrder.OrderItems ?? new List<Models.ServiceOrderItems>();
			serviceOrder.OrderItems.RemoveAll(x => x.ServiceOrderItem.ID == soi.ServiceOrderItem.ID);
			serviceOrder.OrderItems.Add(soi);

			dataHelperServiceOrder.CreateOrUpdate(serviceOrder);
		}

		private void CreateServiceSpecification(LupoAcquisitionData.Root model, string bookingManager, string bookingWizardScript)
		{
			var dataHelperServiceSpecification = new DataHelperServiceSpecification(Engine.SLNetRaw);
			var existingSpec = dataHelperServiceSpecification.Read().Find(x => x.Name == model.Details.Type);

			var spec = new Models.ServiceSpecification
			{
				ID = existingSpec?.ID ?? Guid.NewGuid(),
				Name = model.Details.Type,
				Configurations = BuildConfigsSpec(model, existingSpec?.Configurations ?? new List<Models.ServiceSpecificationConfigurationValue>()),
				Properties = BuildProperties(model),
				ServiceItems = new List<Models.ServiceItem>
				{
					new Models.ServiceItem
					{
						ID = 1,
						Label = model.Details.Type,
						Script = bookingWizardScript,
						DefinitionReference = bookingManager,
						Type = SlcServicemanagementIds.Enums.ServiceitemtypesEnum.SRMBooking,
					},
				},
			};

			dataHelperServiceSpecification.CreateOrUpdate(spec);
		}

		private void RunSafe(IEngine engine)
		{
			string rawBody = engine.GetScriptParam("Data")?.Value;
			if (String.IsNullOrEmpty(rawBody) || !rawBody.StartsWith("{"))
			{
				IngestDummyData();
			}
			else
			{
				var model = JsonConvert.DeserializeObject<LupoAcquisitionData.Root>(rawBody)
								 ?? throw new InvalidOperationException("The input could not be parsed to a valid Acquisition Service.");
				ImportAcquisitionDataAsServiceOrder(model);
			}
		}

		private void IngestDummyData()
		{
			var satDlModel = JsonConvert.DeserializeObject<LupoAcquisitionData.Root>(AcquisitionData1)
							 ?? throw new InvalidOperationException("SAT-DL Data could not be parsed.");
			var radioModel = JsonConvert.DeserializeObject<LupoAcquisitionData.Root>(AcquisitionData2)
							 ?? throw new InvalidOperationException("Radio Data could not be parsed.");
			var sdiModel = JsonConvert.DeserializeObject<LupoAcquisitionData.Root>(AcquisitionData3)
						   ?? throw new InvalidOperationException("Radio Data could not be parsed.");
			var peerModel = JsonConvert.DeserializeObject<LupoAcquisitionData.Root>(AcquisitionData4)
						   ?? throw new InvalidOperationException("Radio Data could not be parsed.");

			CreateServiceSpecification(satDlModel, "SAT-DL Booking Manager", "SRM_CreateNewBooking");
			CreateServiceSpecification(radioModel, "RADIO Booking Manager", "SRM_CreateNewBooking");
			CreateServiceSpecification(sdiModel, "SDI PRE-ENCODING Booking Manager", "SRM_CreateNewBooking");
			CreateServiceSpecification(peerModel, "IP Peering Booking Manager", "SRM_CreateNewBooking");

			ImportAcquisitionDataAsServiceOrder(satDlModel);
			ImportAcquisitionDataAsServiceOrder(radioModel);
			ImportAcquisitionDataAsServiceOrder(sdiModel);
			ImportAcquisitionDataAsServiceOrder(peerModel);
		}
	}
}