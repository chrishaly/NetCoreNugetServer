namespace NugetServer.Controllers
{
	public class IndexResult
	{
		public int UpdatedCount { get { return Updated.Length; } }

		public string[] Updated { get; set; }

		public int AddedCount { get { return Added.Length; } }

		public string[] Added { get; set; }

		public int DeletedCount { get { return Deleted.Length; } }

		public string[] Deleted { get; set; }
	}
}