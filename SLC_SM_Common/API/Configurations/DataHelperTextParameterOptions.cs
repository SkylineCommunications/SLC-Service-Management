namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcConfigurations;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <inheritdoc />
	public class DataHelperTextParameterOptions : DataHelper<Models.TextParameterOptions>
	{
		/// <inheritdoc />
		public DataHelperTextParameterOptions(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.TextParameterOptions)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.TextParameterOptions item)
		{
			var instance = new TextParameterOptionsInstance(New(item.ID));
			instance.TextParameterOptions.Default = item.Default;
			instance.TextParameterOptions.Regex = item.Regex;
			instance.TextParameterOptions.UserMessage = item.UserMessage;

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
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

		/// <inheritdoc />
		public override bool TryDelete(Models.TextParameterOptions item)
		{
			return TryDelete(item.ID);
		}
	}
}