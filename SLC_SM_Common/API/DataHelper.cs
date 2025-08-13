namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <summary>
	///     Provides helper methods for reading, creating, updating, and deleting DOM instances of type
	///     <typeparamref name="T" />.
	/// </summary>
	/// <typeparam name="T">The type of the DOM instance.</typeparam>
	public abstract class DataHelper<T> where T : class
	{
		internal readonly IConnection _connection;
		internal readonly DomDefinitionId _defId;
		internal readonly DomHelper _domHelper;

		/// <summary>
		///     Initializes a new instance of the <see cref="DataHelper{T}" /> class.
		/// </summary>
		/// <param name="connection">The connection to use for DOM operations.</param>
		/// <param name="defId">The DOM definition identifier.</param>
		protected DataHelper(IConnection connection, DomDefinitionId defId)
		{
			_connection = connection;
			_defId = defId;
			_domHelper = new DomHelper(connection.HandleMessages, defId.ModuleId);
		}

		/// <summary>
		///     Creates a new DOM instance or updates an existing one based on the provided item.
		/// </summary>
		/// <param name="item">The item to create or update.</param>
		/// <returns>The unique identifier of the created or updated DOM instance.</returns>
		public abstract Guid CreateOrUpdate(T item);

		/// <summary>
		///     Reads all DOM instances of type <typeparamref name="T" />.
		/// </summary>
		/// <returns>A list of DOM instances of type <typeparamref name="T" />.</returns>
		public abstract List<T> Read();

		/// <summary>
		///     Attempts to delete the specified DOM instance of type <typeparamref name="T" />.
		/// </summary>
		/// <param name="item">The item to delete.</param>
		/// <returns><c>true</c> if the item was successfully deleted; otherwise, <c>false</c>.</returns>
		public abstract bool TryDelete(T item);

		internal Guid CreateOrUpdateInstance(DomInstance instance)
		{
			var result = _domHelper.DomInstances.CreateOrUpdate(new List<DomInstance> { instance });
			return result.SuccessfulIds.FirstOrDefault()?.Id
			       ?? throw new InvalidOperationException($"Failed to create the item due to: {JsonConvert.SerializeObject(result.TraceDataPerItem)}");
		}

		internal DomInstance New(Guid id)
		{
			if (id == Guid.Empty)
			{
				id = Guid.NewGuid();
			}

			return new DomInstance { ID = new DomInstanceId(id), DomDefinitionId = _defId };
		}

		internal bool TryDelete(Guid id)
		{
			return _domHelper.DomInstances.TryDelete(new DomInstance { ID = new DomInstanceId(id) });
		}
	}
}