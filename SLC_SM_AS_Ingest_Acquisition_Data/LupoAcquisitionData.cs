namespace ACQ_LIB
{
	using System;
	using System.Collections.Generic;

	using Newtonsoft.Json;

	public static class LupoAcquisitionData
	{
		public class Full
		{
			[JsonProperty("data")]
			public List<Root> Data { get; set; }
		}

		public class Root
		{
			[JsonProperty("id")]
			public string Id { get; set; }

			[JsonProperty("referenceProgramIds")]
			public List<long> ReferenceProgramIds { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("tenant")]
			public string Tenant { get; set; }

			[JsonProperty("sourceId")]
			public string SourceId { get; set; }

			[JsonProperty("sourceType")]
			public string SourceType { get; set; }

			[JsonProperty("details")]
			public Details Details { get; set; }

			[JsonProperty("readyForProvision")]
			public bool ReadyForProvision { get; set; }

			[JsonProperty("programs")]
			public List<object> Programs { get; set; }

			[JsonProperty("deploymentAt")]
			public string DeploymentAt { get; set; }
		}

		public class Details
		{
			[JsonProperty("usecase")]
			public string Usecase { get; set; }

			[JsonProperty("_id")]
			public string Id { get; set; }

			[JsonProperty("cbr", NullValueHandling = NullValueHandling.Ignore)]
			public string Cbr { get; set; }

			[JsonProperty("type")]
			public string Type { get; set; }

			[JsonProperty("main")]
			public bool Main { get; set; }

			[JsonProperty("encrypted")]
			public bool Encrypted { get; set; }

			[JsonProperty("is_radio")]
			public bool? IsRadio { get; set; }

			[JsonProperty("eit")]
			public bool Eit { get; set; }

			[JsonProperty("eit_pf")]
			public bool EitPf { get; set; }

			[JsonProperty("transparent_mpts")]
			public bool TransparentMpts { get; set; }

			[JsonProperty("service_type")]
			public long ServiceType { get; set; }

			[JsonProperty("tenant_ids")]
			public List<long> TenantIds { get; set; }

			[JsonProperty("service_provider")]
			public List<long> ServiceProvider { get; set; }

			[JsonProperty("source_asi")]
			public object SourceAsi { get; set; }

			[JsonProperty("source_fm")]
			public object SourceFm { get; set; }

			[JsonProperty("source_dvbt2")]
			public object SourceDvbt2 { get; set; }

			[JsonProperty("source_ip")]
			public object SourceIp { get; set; }

			[JsonProperty("source_sat")]
			public SourceSat SourceSat { get; set; }

			[JsonProperty("source_sdi")]
			public SourceSdi SourceSdi { get; set; }

			[JsonProperty("source_srt")]
			public object SourceSrt { get; set; }

			[JsonProperty("source_stream")]
			public SourceStream SourceStream { get; set; }

			[JsonProperty("programs")]
			public List<long> Programs { get; set; }

			[JsonProperty("additional_information")]
			public object AdditionalInformation { get; set; }

			[JsonProperty("pids")]
			public object Pids { get; set; }

			[JsonProperty("forcedOutput", NullValueHandling = NullValueHandling.Ignore)]
			public ForcedOutput ForcedOutput { get; set; }

			[JsonProperty("igmpJoin", NullValueHandling = NullValueHandling.Ignore)]
			public bool? IgmpJoin { get; set; }

			[JsonProperty("sid", NullValueHandling = NullValueHandling.Ignore)]
			public long? Sid { get; set; }
		}

		public class ForcedOutput
		{
			[JsonProperty("outputSid")]
			public long OutputSid { get; set; }

			[JsonProperty("outputServiceName")]
			public string OutputServiceName { get; set; }
		}

		public class SourceSat
		{
			[JsonProperty("guid")]
			public string Guid { get; set; }

			[JsonProperty("updated_at")]
			public string UpdatedAt { get; set; }

			[JsonProperty("created_at")]
			public string CreatedAt { get; set; }

			[JsonProperty("orbital_position")]
			public string OrbitalPosition { get; set; }

			[JsonProperty("polarisation")]
			public string Polarisation { get; set; }

			[JsonProperty("frequency")]
			public long Frequency { get; set; }

			[JsonProperty("symbol_rate")]
			public long SymbolRate { get; set; }

			[JsonProperty("transponder")]
			public string Transponder { get; set; }

			[JsonProperty("standard")]
			public string Standard { get; set; }

			[JsonProperty("modulation")]
			public string Modulation { get; set; }

			[JsonProperty("fec")]
			public string Fec { get; set; }

			[JsonProperty("nid")]
			public long Nid { get; set; }

			[JsonProperty("tid")]
			public long Tid { get; set; }

			[JsonProperty("isi")]
			public object Isi { get; set; }

			[JsonProperty("gsi")]
			public object Gsi { get; set; }

			[JsonProperty("sid")]
			public long Sid { get; set; }
		}

		public class SourceStream
		{
			[JsonProperty("guid")]
			public string Guid { get; set; }

			[JsonProperty("updated_at")]
			public string UpdatedAt { get; set; }

			[JsonProperty("created_at")]
			public string CreatedAt { get; set; }

			[JsonProperty("url_1a")]
			public Uri Url1A { get; set; }

			[JsonProperty("location_1a")]
			public Location Location1A { get; set; }

			[JsonProperty("url_1b")]
			public Uri Url1B { get; set; }

			[JsonProperty("location_1b")]
			public Location Location1B { get; set; }

			[JsonProperty("url_2a")]
			public object Url2A { get; set; }

			[JsonProperty("location_2a")]
			public Location Location2A { get; set; }

			[JsonProperty("url_2b")]
			public object Url2B { get; set; }

			[JsonProperty("location_2b")]
			public Location Location2B { get; set; }
		}

		public class SourceSdi
		{
			[JsonProperty("guid")]
			public string Guid { get; set; }

			[JsonProperty("updated_at")]
			public string UpdatedAt { get; set; }

			[JsonProperty("created_at")]
			public string CreatedAt { get; set; }

			[JsonProperty("standard_1")]
			public string Standard1 { get; set; }

			[JsonProperty("physical_port_1")]
			public string PhysicalPort1 { get; set; }

			[JsonProperty("location_1")]
			public Location Location1 { get; set; }

			[JsonProperty("standard_2")]
			public string Standard2 { get; set; }

			[JsonProperty("physical_port_2")]
			public string PhysicalPort2 { get; set; }

			[JsonProperty("location_2")]
			public Location Location2 { get; set; }

			[JsonProperty("standard_3")]
			public object Standard3 { get; set; }

			[JsonProperty("physical_port_3")]
			public object PhysicalPort3 { get; set; }

			[JsonProperty("location_3")]
			public object Location3 { get; set; }

			[JsonProperty("standard_4")]
			public object Standard4 { get; set; }

			[JsonProperty("physical_port_4")]
			public object PhysicalPort4 { get; set; }

			[JsonProperty("location_4")]
			public object Location4 { get; set; }
		}

		public class Location
		{
			[JsonProperty("host_prefix")]
			public string HostPrefix { get; set; }
		}
	}
}