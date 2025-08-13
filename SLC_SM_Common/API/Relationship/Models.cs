namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.Relationship
{
	using System;

	public static class Models
	{
		public class Link
		{
			public Guid ID { get; set; }

			public string ChildID { get; set; }

			public string ChildName { get; set; }

			public string ParentID { get; set; }

			public string ParentName { get; set; }
		}
	}
}