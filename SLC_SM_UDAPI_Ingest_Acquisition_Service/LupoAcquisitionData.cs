namespace ACQ_LIB
{
	using System.Collections.Generic;

	using Newtonsoft.Json;

	public static class LupoAcquisitionData
	{
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
			public object SourceSat { get; set; }

			[JsonProperty("source_sdi")]
			public object SourceSdi { get; set; }

			[JsonProperty("source_srt")]
			public object SourceSrt { get; set; }

			[JsonProperty("source_stream")]
			public object SourceStream { get; set; }

			[JsonProperty("programs")]
			public List<long> Programs { get; set; }

			[JsonProperty("additional_information")]
			public object AdditionalInformation { get; set; }

			[JsonProperty("pids")]
			public object Pids { get; set; }

			[JsonProperty("forcedOutput", NullValueHandling = NullValueHandling.Ignore)]
			public object ForcedOutput { get; set; }

			[JsonProperty("igmpJoin", NullValueHandling = NullValueHandling.Ignore)]
			public bool? IgmpJoin { get; set; }

			[JsonProperty("sid", NullValueHandling = NullValueHandling.Ignore)]
			public long? Sid { get; set; }
		}
	}
}