namespace NugetServer.Models
{
	public class QueryParameters
	{
		public string SearchTerm { get; set; }

		public string TargetFramework { get; set; }

		public bool IncludePrerelease { get; set; }

		public string OrderBy { get; set; }

		public bool OrderByDescending { get; set; }

		public int Skip { get; set; }

		public int Take { get; set; }

		public bool IsLatestVersion { get; set; }

		public bool IsAbsoluteLatestVersion { get; set; }

		public string[] SelectedFields { get; set; }
	}
}