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

	using SLC_SM_Common.API.ConfigurationsApi;
	using SLC_SM_Common.API.PeopleAndOrganizationApi;
	using SLC_SM_Common.API.ServiceManagementApi;

	using Models = SLC_SM_Common.API.ServiceManagementApi.Models;

	/// <summary>
	///     Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		private static readonly string AcquisitionData1 =
			"{\"id\":\"VFCZ:4440\",\"referenceProgramIds\":[9259,9175],\"name\":\"BBC_News_Europe/SAT/1/V02\",\"tenant\":\"VFCZ\",\"sourceId\":\"f3e32bb68f124c84e054304dc0cb042da969fa3c\",\"sourceType\":\"SAT\",\"details\":{\"usecase\":\"SATDL - FTA Channels\",\"_id\":\"682de70f23a1002e1bc6007c\",\"updated_at\":\"2025-06-03T15:17:58+02:00\",\"created_at\":\"2025-06-03T15:17:58+02:00\",\"type\":\"SAT\",\"complete\":true,\"main\":true,\"encrypted\":false,\"is_radio\":false,\"eit\":false,\"eit_pf\":false,\"transparent_mpts\":false,\"service_type\":22,\"service_type_label\":\"AVCSDTV (0x16)\",\"tenant_ids\":[1,2,6],\"service_provider\":[639],\"usage_type\":\"ALL\",\"sid_1\":13104,\"sid_2\":null,\"sid_3\":null,\"sid_4\":null,\"source_asi\":null,\"source_fm\":null,\"source_dvbt2\":null,\"source_ip\":null,\"source_sat\":{\"guid\":\"f3e32bb68f124c84e054304dc0cb042da969fa3c\",\"updated_at\":\"2025-06-03T15:17:35+02:00\",\"created_at\":\"2025-06-03T15:17:35+02:00\",\"orbital_position\":\"13,0° Ost; Hot Bird 13F\",\"polarisation\":\"V\",\"frequency\":11727,\"symbol_rate\":29900,\"transponder\":\"50\",\"standard\":\"DVB-S2\",\"modulation\":\"8PSK\",\"fec\":\"3/4\",\"nid\":318,\"tid\":5000,\"isi\":null,\"gsi\":null,\"sid\":13104},\"source_sdi\":null,\"source_srt\":null,\"source_stream\":null,\"programs\":[9259,9175],\"additional_information\":null,\"pids\":{\"video\":1040,\"pmt\":104,\"pcr\":1040,\"teletext\":1042,\"subtitle\":null,\"ait_hbbtv\":0,\"eit\":0,\"sdt\":0,\"audio\":[]},\"notice\":null},\"readyForProvision\":false,\"usageType\":\"ALL\",\"createdAt\":\"2025-05-21T14:45:35.632Z\",\"updatedAt\":\"2025-05-21T14:45:35.632Z\",\"programs\":[]}";
		private static readonly string AcquisitionData2 =
			"{\"id\":\"VFDE:4432\",\"referenceProgramIds\":[1164],\"name\":\"Antenne_Thueringen_(Mitte)/STREAM/1/V02\",\"tenant\":\"VFDE\",\"sourceId\":\"854b0551a1327a9198ade5b37718a18bdc4a41ce\",\"sourceType\":\"Stream\",\"details\":{\"usecase\":\"Qbit Ipstream Radio Channels\",\"_id\":\"682de70f23a1002e1bc60312\",\"updated_at\":\"2025-06-03T15:17:58+02:00\",\"created_at\":\"2025-06-03T15:17:58+02:00\",\"type\":\"Stream\",\"complete\":true,\"main\":true,\"encrypted\":false,\"is_radio\":true,\"eit\":false,\"eit_pf\":false,\"transparent_mpts\":false,\"service_type\":2,\"service_type_label\":\"Radio (0x02)\",\"tenant_ids\":[1],\"service_provider\":[],\"usage_type\":\"ALL\",\"sid_1\":0,\"sid_2\":null,\"sid_3\":null,\"sid_4\":null,\"source_asi\":null,\"source_fm\":null,\"source_dvbt2\":null,\"source_ip\":null,\"source_sat\":null,\"source_sdi\":null,\"source_srt\":null,\"source_stream\":{\"guid\":\"854b0551a1327a9198ade5b37718a18bdc4a41ce\",\"updated_at\":\"2025-06-03T15:17:58+02:00\",\"created_at\":\"2025-06-03T15:17:58+02:00\",\"url_1a\":\"http://dist-01.audiomediaplus.de/atmitte_256k\",\"location_1a\":{\"id\":989,\"region\":\"OTHER\",\"name\":\"PoC Kerpen\",\"loc_type\":4,\"loc_type_label\":\"Foreign site\",\"host_prefix\":\"bm-mich\",\"address\":null,\"room\":null,\"postcode\":null,\"city\":null},\"url_1b\":\"http://dist-01.audiomediaplus.de/atmitte_256k\",\"location_1b\":{\"id\":1024,\"region\":\"OTHER\",\"name\":\"PoC Rödelheim\",\"loc_type\":4,\"loc_type_label\":\"Foreign site\",\"host_prefix\":\"f-brei\",\"address\":null,\"room\":null,\"postcode\":null,\"city\":null},\"url_2a\":null,\"location_2a\":null,\"url_2b\":null,\"location_2b\":null},\"programs\":[1164],\"additional_information\":null,\"pids\":{\"video\":0,\"pmt\":0,\"pcr\":0,\"teletext\":0,\"subtitle\":null,\"ait_hbbtv\":0,\"eit\":null,\"sdt\":null,\"audio\":[]},\"notice\":null},\"readyForProvision\":false,\"usageType\":\"ALL\",\"createdAt\":\"2025-05-21T14:45:35.645Z\",\"updatedAt\":\"2025-05-21T14:45:35.645Z\",\"programs\":[]}";
		private static readonly string AcquisitionData3 = 
			"{\"id\":\"VFDE:3312\",\"referenceProgramIds\":[434,7895],\"name\":\"ARD_alpha_HD/LTGA3/7.2B\",\"tenant\":\"VFDE\",\"sourceId\":\"88879149e783e554601e2089f06165085bb206cc\",\"sourceType\":\"SDI\",\"details\":{\"usecase\":\"SDI Backhaul Channels\",\"_id\":\"682de70f23a1002e1bc600ae\",\"updated_at\":\"2025-05-21T15:03:33+02:00\",\"created_at\":\"2025-05-21T15:03:33+02:00\",\"type\":\"SDI\",\"complete\":true,\"main\":true,\"encrypted\":false,\"is_radio\":false,\"eit\":false,\"eit_pf\":false,\"transparent_mpts\":false,\"service_type\":25,\"service_type_label\":\"HDTV (0x19)\",\"tenant_ids\":[1],\"service_provider\":[],\"usage_type\":\"ALL\",\"sid_1\":null,\"sid_2\":null,\"sid_3\":null,\"sid_4\":null,\"source_asi\":null,\"source_fm\":null,\"source_dvbt2\":null,\"source_ip\":null,\"source_sat\":null,\"source_sdi\":{\"guid\":\"88879149e783e554601e2089f06165085bb206cc\",\"updated_at\":\"2025-05-21T15:03:33+02:00\",\"created_at\":\"2025-05-21T15:03:33+02:00\",\"standard_1\":\"HD-SDI\",\"physical_port_1\":\"7.2B\",\"location_1\":{\"id\":1012,\"region\":\"OTHER\",\"name\":\"Potsdam A, Marlene-Dietrich-Allee 20, 14882 Potsdam, Raum Sendezentrum Fernsehen (SZF), Hybnet-Raum \",\"loc_type\":4,\"loc_type_label\":\"Foreign site\",\"host_prefix\":\"p-marl-x20-ard-1\",\"address\":null,\"room\":null,\"postcode\":null,\"city\":null},\"standard_2\":\"HD-SDI\",\"physical_port_2\":\"7.2B\",\"location_2\":{\"id\":1034,\"region\":\"OTHER\",\"name\":\"Potsdam B, Marlene-Dietrich-Allee 20, 14882 Potsdam, Raum Radiohaus (RDH), ZGR, p-marl-x20-ard-2\",\"loc_type\":4,\"loc_type_label\":\"Foreign site\",\"host_prefix\":\"p-marl-x20-ard-2\",\"address\":null,\"room\":null,\"postcode\":null,\"city\":null},\"standard_3\":null,\"physical_port_3\":null,\"location_3\":null,\"standard_4\":null,\"physical_port_4\":null,\"location_4\":null},\"source_srt\":null,\"source_stream\":null,\"programs\":[434,7895],\"additional_information\":null,\"pids\":{\"video\":null,\"pmt\":null,\"pcr\":null,\"teletext\":null,\"subtitle\":null,\"ait_hbbtv\":null,\"eit\":null,\"sdt\":null,\"audio\":[]},\"notice\":\"SDI Pre-Enc\"},\"readyForProvision\":false,\"usageType\":\"ALL\",\"createdAt\":\"2025-05-21T14:45:35.633Z\",\"updatedAt\":\"2025-05-21T14:45:35.633Z\",\"programs\":[]}";

		private Action<string> _logger;

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

		private static List<Models.ServiceOrderItemConfigurationValue> BuildConfigs(LupoAcquisitionData.Root model, List<Tuple<Guid, Guid>> existingValues)
		{
			switch (model.Details.Type)
			{
				case "SAT":
					return BuildConfigsSatDl(model, existingValues);

				case "Stream":
					return BuildConfigsRadio(model, existingValues);

				case "SDI":
					return BuildConfigsSdiPenc(model, existingValues);

				default:
					throw new NotImplementedException($"Type '{model.Details.Type}' not implemented (yet).");
			}
		}

		private static List<Models.ServiceOrderItemConfigurationValue> BuildConfigsSatDl(LupoAcquisitionData.Root model, List<Tuple<Guid, Guid>> existingValues)
		{
			var dataHelper = new DataHelperConfigurationParameterValue(Engine.SLNetRaw);
			var existingConfigParams = dataHelper.Read();

			var paramPolarization = existingConfigParams.Find(x => x.Label == "Polarization") ?? CreateConfigPolarization(dataHelper, "Polarization");
			paramPolarization.StringValue = model.Details.SourceSat.Polarisation.ToUpper().StartsWith("V") ? "Vertical" : "Horizontal";

			var paramOrbital = existingConfigParams.Find(x => x.Label == "Orbital Position") ?? CreateConfigOrbitalPosition(dataHelper, "Orbital Position");
			var orbitalOptions = CreateConfigOrbitalPosition(dataHelper, "").DiscreteOptions.DiscreteValues.Select(x => x.Value).ToList();
			paramOrbital.StringValue = orbitalOptions.First(x => x.StartsWith(model.Details.SourceSat.OrbitalPosition.Replace(",", ".").Split('°')[0]));

			var paramFreq = existingConfigParams.Find(x => x.Label == "Frequency") ?? CreateConfigFrequency(dataHelper, "Frequency");
			paramFreq.DoubleValue = model.Details.SourceSat.Frequency / 1_000;

			var paramSymbolRate = existingConfigParams.Find(x => x.Label == "Symbol Rate") ?? CreateConfigSymbolRate(dataHelper, "Symbol Rate");
			paramSymbolRate.DoubleValue = model.Details.SourceSat.SymbolRate / 1_000;

			var paramSid = existingConfigParams.Find(x => x.Label == "Service ID") ?? CreateConfigServiceId(dataHelper, "Service ID");
			paramSid.DoubleValue = model.Details.SourceSat.Sid;

			var paramTransponder = existingConfigParams.Find(x => x.Label == "Transponder") ?? CreateConfigTransponder(dataHelper, "Transponder");
			paramTransponder.StringValue = model.Details.SourceSat.Transponder;

			return new List<Models.ServiceOrderItemConfigurationValue>
			{
				new Models.ServiceOrderItemConfigurationValue
				{
					ID = existingValues.Find(x => x.Item2 == paramPolarization.ConfigurationParameterId)?.Item1 ?? Guid.NewGuid(),
					Mandatory = true,
					ConfigurationParameter = paramPolarization,
				},
				new Models.ServiceOrderItemConfigurationValue
				{
					ID = existingValues.Find(x => x.Item2 == paramFreq.ConfigurationParameterId)?.Item1 ?? Guid.NewGuid(),
					Mandatory = true,
					ConfigurationParameter = paramFreq,
				},
				new Models.ServiceOrderItemConfigurationValue
				{
					ID = existingValues.Find(x => x.Item2 == paramSymbolRate.ConfigurationParameterId)?.Item1 ?? Guid.NewGuid(),
					Mandatory = true,
					ConfigurationParameter = paramSymbolRate,
				},
				new Models.ServiceOrderItemConfigurationValue
				{
					ID = existingValues.Find(x => x.Item2 == paramSid.ConfigurationParameterId)?.Item1 ?? Guid.NewGuid(),
					Mandatory = true,
					ConfigurationParameter = paramSid,
				},
				new Models.ServiceOrderItemConfigurationValue
				{
					ID = existingValues.Find(x => x.Item2 == paramOrbital.ConfigurationParameterId)?.Item1 ?? Guid.NewGuid(),
					Mandatory = true,
					ConfigurationParameter = paramOrbital,
				},
				new Models.ServiceOrderItemConfigurationValue
				{
					ID = existingValues.Find(x => x.Item2 == paramTransponder.ConfigurationParameterId)?.Item1 ?? Guid.NewGuid(),
					Mandatory = true,
					ConfigurationParameter = paramTransponder,
				},
			};
		}

		private static List<Models.ServiceOrderItemConfigurationValue> BuildConfigsRadio(LupoAcquisitionData.Root model, List<Tuple<Guid, Guid>> existingValues)
		{
			var dataHelper = new DataHelperConfigurationParameterValue(Engine.SLNetRaw);
			var existingConfigParams = dataHelper.Read();

			var paramSid = existingConfigParams.Find(x => x.Label == "Service ID") ?? CreateConfigServiceId(dataHelper, "Service ID");
			paramSid.DoubleValue = model.Details.Programs.FirstOrDefault();

			var paramMainUrl = existingConfigParams.Find(x => x.Label == "URL Main") ?? CreateConfigUrl(dataHelper, "URL Main");
			paramMainUrl.StringValue = model.Details.SourceStream.Url1A?.ToString() ?? String.Empty;

			var paramBkupUrl = existingConfigParams.Find(x => x.Label == "URL Backup") ?? CreateConfigUrl(dataHelper, "URL Backup");
			paramBkupUrl.StringValue = model.Details.SourceStream.Url1B?.ToString() ?? String.Empty;

			return new List<Models.ServiceOrderItemConfigurationValue>
			{
				new Models.ServiceOrderItemConfigurationValue
				{
					ID = existingValues.Find(x => x.Item2 == paramSid.ConfigurationParameterId)?.Item1 ?? Guid.NewGuid(),
					Mandatory = true,
					ConfigurationParameter = paramSid,
				},
				new Models.ServiceOrderItemConfigurationValue
				{
					ID = existingValues.Find(x => x.Item2 == paramMainUrl.ConfigurationParameterId)?.Item1 ?? Guid.NewGuid(),
					Mandatory = true,
					ConfigurationParameter = paramMainUrl,
				},
				new Models.ServiceOrderItemConfigurationValue
				{
					ID = existingValues.Find(x => x.Item2 == paramBkupUrl.ConfigurationParameterId)?.Item1 ?? Guid.NewGuid(),
					Mandatory = true,
					ConfigurationParameter = paramBkupUrl,
				},
			};
		}

		private static List<Models.ServiceOrderItemConfigurationValue> BuildConfigsSdiPenc(LupoAcquisitionData.Root model, List<Tuple<Guid, Guid>> existingValues)
		{
			var dataHelper = new DataHelperConfigurationParameterValue(Engine.SLNetRaw);
			var existingConfigParams = dataHelper.Read();

			var paramMainUrl = existingConfigParams.Find(x => x.Label == "Interface Main") ?? CreateConfigUrl(dataHelper, "Interface Main");
			paramMainUrl.StringValue = model.Details.SourceSdi.PhysicalPort1 ?? String.Empty;

			var paramBkupUrl = existingConfigParams.Find(x => x.Label == "Interface Backup") ?? CreateConfigUrl(dataHelper, "Interface Backup");
			paramBkupUrl.StringValue = model.Details.SourceSdi.PhysicalPort2 ?? String.Empty;

			var paramMainDevice = existingConfigParams.Find(x => x.Label == "Device Location 1") ?? CreateConfigUrl(dataHelper, "Device Location 1");
			paramMainDevice.StringValue = model.Details.SourceSdi.Location1?.HostPrefix ?? String.Empty;

			var paramBkupDevice = existingConfigParams.Find(x => x.Label == "Device Location 2") ?? CreateConfigUrl(dataHelper, "Device Location 2");
			paramBkupDevice.StringValue = model.Details.SourceSdi.Location2?.HostPrefix ?? String.Empty;

			return new List<Models.ServiceOrderItemConfigurationValue>
			{
				new Models.ServiceOrderItemConfigurationValue
				{
					ID = existingValues.Find(x => x.Item2 == paramMainUrl.ConfigurationParameterId)?.Item1 ?? Guid.NewGuid(),
					Mandatory = true,
					ConfigurationParameter = paramMainUrl,
				},
				new Models.ServiceOrderItemConfigurationValue
				{
					ID = existingValues.Find(x => x.Item2 == paramBkupUrl.ConfigurationParameterId)?.Item1 ?? Guid.NewGuid(),
					Mandatory = true,
					ConfigurationParameter = paramBkupUrl,
				},
				new Models.ServiceOrderItemConfigurationValue
				{
					ID = existingValues.Find(x => x.Item2 == paramMainDevice.ConfigurationParameterId)?.Item1 ?? Guid.NewGuid(),
					Mandatory = true,
					ConfigurationParameter = paramMainDevice,
				},
				new Models.ServiceOrderItemConfigurationValue
				{
					ID = existingValues.Find(x => x.Item2 == paramBkupDevice.ConfigurationParameterId)?.Item1 ?? Guid.NewGuid(),
					Mandatory = true,
					ConfigurationParameter = paramBkupDevice,
				},
			};
		}

		private static Models.ServicePropertyValues BuildProperties(LupoAcquisitionData.Root model)
		{
			var dataHelper = new DataHelperServiceProperties(Engine.SLNetRaw);
			var existingProperties = dataHelper.Read();

			var propTenantId = existingProperties.Find(x => x.Name == "Tenant")?.ID ?? CreateProperty(dataHelper, "Tenant");

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

		private static SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameterValue CreateConfigFrequency(DataHelperConfigurationParameterValue helper, string name)
		{
			var id = helper.CreateOrUpdate(
				new SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameterValue
				{
					Label = name,
					Type = SlcConfigurationsIds.Enums.ParameterType.Number,
					NumberOptions = new SLC_SM_Common.API.ConfigurationsApi.Models.NumberParameterOptions
					{
						Decimals = 6,
						MinRange = 0,
						Stepsize = 0.000001,
						DefaultUnit = new SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationUnit { Name = "MHz" },
						Units = new List<SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationUnit>
						{
							new SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationUnit { Name = "MHz" },
						},
					},
				});
			return helper.Read().First(x => x.ID == id);
		}

		private static SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameterValue CreateConfigOrbitalPosition(DataHelperConfigurationParameterValue helper, string name)
		{
			var id = helper.CreateOrUpdate(
				new SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameterValue
				{
					Label = name,
					Type = SlcConfigurationsIds.Enums.ParameterType.Discrete,
					DiscreteOptions = new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteParameterOptions
					{
						Default = new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "19.2E" },
						DiscreteValues = new List<SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue>
						{
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "0.8W" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "3.0E" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "4.8E" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "5.0W" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "7.0E" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "9.0E" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "12.5W" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "13.0E" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "15.0W" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "16.0E" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "19.2E" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "21.5E" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "23.5E" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "26.0E" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "28.2E" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "30.5E" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "33.0E" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "34.5W" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "36.0E" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "39.0E" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "42.0E" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "48.0E" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "55.0E" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "68.5E" },
						},
					},
				});
			return helper.Read().First(x => x.ID == id);
		}

		private static SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameterValue CreateConfigPolarization(DataHelperConfigurationParameterValue helper, string name)
		{
			var id = helper.CreateOrUpdate(
				new SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameterValue
				{
					Label = name,
					Type = SlcConfigurationsIds.Enums.ParameterType.Discrete,
					DiscreteOptions = new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteParameterOptions
					{
						Default = new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "Horizontal" },
						DiscreteValues = new List<SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue>
						{
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "Horizontal" },
							new SLC_SM_Common.API.ConfigurationsApi.Models.DiscreteValue { Value = "Vertical" },
						},
					},
				});
			return helper.Read().First(x => x.ID == id);
		}

		private static SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameterValue CreateConfigServiceId(DataHelperConfigurationParameterValue helper, string name)
		{
			var id = helper.CreateOrUpdate(
				new SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameterValue
				{
					Label = name,
					Type = SlcConfigurationsIds.Enums.ParameterType.Number,
					NumberOptions = new SLC_SM_Common.API.ConfigurationsApi.Models.NumberParameterOptions
					{
						MinRange = 0,
						MaxRange = 50000,
						Stepsize = 1,
						Decimals = 0,
					},
				});
			return helper.Read().First(x => x.ID == id);
		}

		private static SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameterValue CreateConfigUrl(DataHelperConfigurationParameterValue helper, string name)
		{
			var id = helper.CreateOrUpdate(
				new SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameterValue
				{
					Label = name,
					Type = SlcConfigurationsIds.Enums.ParameterType.Text,
				});
			return helper.Read().First(x => x.ID == id);
		}

		private static SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameterValue CreateConfigSymbolRate(DataHelperConfigurationParameterValue helper, string name)
		{
			var id = helper.CreateOrUpdate(
				new SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameterValue
				{
					Label = name,
					Type = SlcConfigurationsIds.Enums.ParameterType.Number,
					NumberOptions = new SLC_SM_Common.API.ConfigurationsApi.Models.NumberParameterOptions
					{
						Decimals = 6,
						MinRange = 0,
						Stepsize = 0.000001,
						DefaultUnit = new SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationUnit { Name = "MBd" },
						Units = new List<SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationUnit>
						{
							new SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationUnit { Name = "MBd" },
						},
					},
				});
			return helper.Read().First(x => x.ID == id);
		}

		private static SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameterValue CreateConfigTransponder(DataHelperConfigurationParameterValue helper, string name)
		{
			var id = helper.CreateOrUpdate(
				new SLC_SM_Common.API.ConfigurationsApi.Models.ConfigurationParameterValue
				{
					Label = name,
					Type = SlcConfigurationsIds.Enums.ParameterType.Text,
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

		private static SLC_SM_Common.API.PeopleAndOrganizationApi.Models.Organization GetOrganization()
		{
			var dataHelperOrg = new SLC_SM_Common.API.PeopleAndOrganizationApi.DataHelperOrganization(Engine.SLNetRaw);
			var org = dataHelperOrg.Read().Find(x => x.Name == "Vodafone Kabel Deutschland");
			if (org == null)
			{
				var dataHelperCat = new SLC_SM_Common.API.PeopleAndOrganizationApi.DataHelperCategory(Engine.SLNetRaw);
				var category = dataHelperCat.Read().Find(x => x.Name == "Content Provider");
				if (category == null)
				{
					category = new SLC_SM_Common.API.PeopleAndOrganizationApi.Models.Category
					{
						ID = Guid.NewGuid(),
						Name = "Content Provider",
					};
					dataHelperCat.CreateOrUpdate(category);
				}

				org = new SLC_SM_Common.API.PeopleAndOrganizationApi.Models.Organization
				{
					ID = Guid.NewGuid(),
					Name = "Vodafone Kabel Deutschland",
					CategoryId = category.ID,
				};
				dataHelperOrg.CreateOrUpdate(org);
			}

			return org;
		}

		private static SLC_SM_Common.API.PeopleAndOrganizationApi.Models.People GetPeople(Guid org)
		{
			var dataHelperPeople = new DataHelperPeople(Engine.SLNetRaw);
			var people = dataHelperPeople.Read().Find(x => x.OrganizationId == org);
			if (people == null)
			{
				var dataHelperExperience = new DataHelperPersonalExperience(Engine.SLNetRaw);
				var exp = dataHelperExperience.Read().Find(x => x.Value == "Master");
				if (exp == null)
				{
					exp = new SLC_SM_Common.API.PeopleAndOrganizationApi.Models.ExperienceLevel
					{
						ID = Guid.NewGuid(),
						Value = "Master",
					};
					dataHelperExperience.CreateOrUpdate(exp);
				}

				people = new SLC_SM_Common.API.PeopleAndOrganizationApi.Models.People
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
			var dataHelperServiceOrder = new DataHelperServiceOrder(Engine.SLNetRaw);

			var org = GetOrganization();
			var contact = GetPeople(org.ID);

			var serviceOrder = dataHelperServiceOrder.Read().Find(x => x.ExternalID == model.Id)
							   ?? new Models.ServiceOrder { ID = Guid.NewGuid(), ExternalID = model.Id };

			var orderItemName = $"{model.Name} [{model.Details.Type}]";
			Models.ServicePropertyValues prop = BuildProperties(model);

			var existingConfigIds = new List<Guid>();
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
					existingConfigIds = orderItem.ServiceOrderItem.Configurations.Where(x => x != null).Select(x => x.ID).ToList();
				}
			}

			var dataHelperValues = new DataHelperServiceOrderItemConfigurationValue(Engine.SLNetRaw);
			var existingValues = dataHelperValues.Read().Where(x => existingConfigIds.Contains(x.ID) && x.ConfigurationParameter != null).Select(x => new Tuple<Guid, Guid>(x.ID, x.ConfigurationParameter.ID)).ToList();
			var configs = BuildConfigs(model, existingValues);

			var dataHelperServiceSpecification = new DataHelperServiceSpecification(Engine.SLNetRaw);
			var specificationId = dataHelperServiceSpecification.Read().Find(x => x.Name == model.Details.Type)?.ID;

			serviceOrder.Name = model.Name;
			serviceOrder.Description = model.Name;
			serviceOrder.Priority = SlcServicemanagementIds.Enums.ServiceorderpriorityEnum.Medium;
			serviceOrder.OrganizationId = org.ID;
			serviceOrder.ContactIds = new List<Guid> { contact.ID };
			serviceOrder.OrderItems = new List<Models.ServiceOrderItems>
			{
				new Models.ServiceOrderItems
				{
					Priority = 1,
					ServiceOrderItem = new Models.ServiceOrderItem
					{
						ID = orderItem?.ServiceOrderItem.ID ?? Guid.NewGuid(),
						Action = "Add",
						Name = orderItemName,
						StartTime = model.DeploymentAt != null ? DateTime.Parse(model.DeploymentAt) : DateTime.UtcNow + TimeSpan.FromDays(7),
						EndTime = default,
						IndefiniteRuntime = true,
						ServiceCategoryId = default,
						Properties = prop,
						Configurations = configs,
						ServiceId = default,
						SpecificationId = specificationId,
					},
				},
			};

			dataHelperServiceOrder.CreateOrUpdate(serviceOrder);
		}

		private void CreateServiceSpecification(LupoAcquisitionData.Root model, string bookingWizardScript)
		{
			var dataHelperServiceSpecification = new DataHelperServiceSpecification(Engine.SLNetRaw);
			if (dataHelperServiceSpecification.Read().Any(x => x.Name == model.Details.Type))
			{
				return;
			}

			var spec = new Models.ServiceSpecification
			{
				ID = Guid.NewGuid(),
				Name = model.Details.Type,
				Configurations = BuildConfigs(model, new List<Tuple<Guid, Guid>>()).Select(x => new Models.ServiceSpecificationConfigurationValue
				{
					ID = x.ID,
					ConfigurationParameter = x.ConfigurationParameter,
					ExposeAtServiceOrder = x.Mandatory,
					MandatoryAtService = x.Mandatory,
					MandatoryAtServiceOrder = x.Mandatory,
				}).ToList(),
				Properties = BuildProperties(model),
				ServiceItems = new List<Models.ServiceItem>
				{
					new Models.ServiceItem
					{
						ID = 1,
						Label = model.Details.Type,
						Script = bookingWizardScript,
						Type = SlcServicemanagementIds.Enums.ServiceitemtypesEnum.SRMBooking,
					},
				},
			};

			dataHelperServiceSpecification.CreateOrUpdate(spec);
		}

		private void RunSafe(IEngine engine)
		{
			var satDlModel = JsonConvert.DeserializeObject<LupoAcquisitionData.Root>(AcquisitionData1)
						?? throw new InvalidOperationException("SAT-DL Data could not be parsed.");

			CreateServiceSpecification(satDlModel, "SAT-DL Booking Wizard");

			//ImportAcquisitionDataAsServiceOrder(satDlModel);

			var radioModel = JsonConvert.DeserializeObject<LupoAcquisitionData.Root>(AcquisitionData2)
						?? throw new InvalidOperationException("Radio Data could not be parsed.");

			CreateServiceSpecification(radioModel, "RADIO Booking Wizard");

			//ImportAcquisitionDataAsServiceOrder(radioModel);

			var sdiModel = JsonConvert.DeserializeObject<LupoAcquisitionData.Root>(AcquisitionData3)
						?? throw new InvalidOperationException("Radio Data could not be parsed.");

			CreateServiceSpecification(sdiModel, "SDI PRE-ENCODING Booking Wizard");

			ImportAcquisitionDataAsServiceOrder(sdiModel);
		}
	}
}