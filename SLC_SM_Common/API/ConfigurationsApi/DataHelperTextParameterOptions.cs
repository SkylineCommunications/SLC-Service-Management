namespace SLC_SM_Common.API.ConfigurationsApi
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcConfigurations;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class DataHelperTextParameterOptions : DataHelper<Models.TextParameterOptions>
	{
		public DataHelperTextParameterOptions(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.TextParameterOptions)
		{
		}

		public override List<Models.TextParameterOptions> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id))
				.Select(x => new TextParameterOptionsInstance(x))
				.ToList();

			return instances.Select(
					x => new Models.TextParameterOptions
					{
						ID = x.ID.Id,
						Regex = x.TextParameterOptions.Regex,
						Default = x.TextParameterOptions.Default,
						UserMessage = x.TextParameterOptions.UserMessage,
					})
				.ToList();
		}

		public override Guid CreateOrUpdate(Models.TextParameterOptions item)
		{
			var instance = new TextParameterOptionsInstance(New(item.ID));
			instance.TextParameterOptions.Default = item.Default;
			instance.TextParameterOptions.Regex = item.Regex;
			instance.TextParameterOptions.UserMessage = item.UserMessage;

			return CreateOrUpdateInstance(instance);
		}
	}
}