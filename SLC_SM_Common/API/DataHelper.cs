namespace SLC_SM_Common.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	public abstract class DataHelper<T> where T : class
	{
		internal readonly IConnection _connection;
		internal readonly DomDefinitionId _defId;
		internal readonly DomHelper _domHelper;

		protected DataHelper(IConnection connection, DomDefinitionId defId)
		{
			_connection = connection;
			_defId = defId;
			_domHelper = new DomHelper(connection.HandleMessages, defId.ModuleId);
		}

		public abstract List<T> Read();

		public abstract Guid CreateOrUpdate(T item);

		public abstract bool TryDelete(T item);

		internal Guid CreateOrUpdateInstance(DomInstance instance)
		{
			var result = _domHelper.DomInstances.CreateOrUpdate(new List<DomInstance> { instance });
			return result.SuccessfulIds.FirstOrDefault()?.Id
				   ?? throw new InvalidOperationException($"Failed to create the item due to: {JsonConvert.SerializeObject(result.TraceDataPerItem)}");
		}

		internal bool TryDelete(Guid id)
		{
			return _domHelper.DomInstances.TryDelete(new DomInstance { ID = new DomInstanceId(id) });
		}

		internal DomInstance New(Guid id)
		{
			if (id == Guid.Empty)
			{
				id = Guid.NewGuid();
			}

			return new DomInstance { ID = new DomInstanceId(id), DomDefinitionId = _defId };
		}
	}
}