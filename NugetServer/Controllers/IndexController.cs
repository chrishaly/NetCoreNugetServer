using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace NugetServer.Controllers
{
	[Produces("application/json")]
	[Route("")]
	[Route("api/v2")]
	public class IndexController : Controller
	{
		public IActionResult Index()
		{
			var baseUrl = $"http://{Request.Host}";
			var content = $@"<service xml:base=""{baseUrl}/""
	xmlns:atom=""http://www.w3.org/2005/Atom""
	xmlns:app=""http://www.w3.org/2007/app""
	xmlns=""http://www.w3.org/2007/app"">
  <workspace>
	<atom:title>Default</atom:title>
	<collection href=""Packages"">
		<atom:title>Packages</atom:title>
	</collection>
  </workspace>
</service>";

			return Content(content, "text/xml", Encoding.UTF8);
		}

		[Route("$metadata")]
		public IActionResult Metadata()
		{
			var content = @"<?xml version=""1.0"" encoding=""utf-8""?><edmx:Edmx Version=""1.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2007/06/edmx""><edmx:DataServices m:DataServiceVersion=""2.0"" m:MaxDataServiceVersion=""2.0"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata""><Schema Namespace=""NuGetGallery.OData"" xmlns=""http://schemas.microsoft.com/ado/2006/04/edm""><EntityType Name=""V2FeedPackage"" m:HasStream=""true""><Key><PropertyRef Name=""Id"" /><PropertyRef Name=""Version"" /></Key><Property Name=""Id"" Type=""Edm.String"" Nullable=""false"" /><Property Name=""Version"" Type=""Edm.String"" Nullable=""false"" /><Property Name=""NormalizedVersion"" Type=""Edm.String"" /><Property Name=""Authors"" Type=""Edm.String"" /><Property Name=""Copyright"" Type=""Edm.String"" /><Property Name=""Created"" Type=""Edm.DateTime"" Nullable=""false"" /><Property Name=""Dependencies"" Type=""Edm.String"" /><Property Name=""Description"" Type=""Edm.String"" /><Property Name=""DownloadCount"" Type=""Edm.Int32"" Nullable=""false"" /><Property Name=""GalleryDetailsUrl"" Type=""Edm.String"" /><Property Name=""IconUrl"" Type=""Edm.String"" /><Property Name=""IsLatestVersion"" Type=""Edm.Boolean"" Nullable=""false"" /><Property Name=""IsAbsoluteLatestVersion"" Type=""Edm.Boolean"" Nullable=""false"" /><Property Name=""IsPrerelease"" Type=""Edm.Boolean"" Nullable=""false"" /><Property Name=""Language"" Type=""Edm.String"" /><Property Name=""LastUpdated"" Type=""Edm.DateTime"" Nullable=""false"" /><Property Name=""Published"" Type=""Edm.DateTime"" Nullable=""false"" /><Property Name=""PackageHash"" Type=""Edm.String"" /><Property Name=""PackageHashAlgorithm"" Type=""Edm.String"" /><Property Name=""PackageSize"" Type=""Edm.Int64"" Nullable=""false"" /><Property Name=""ProjectUrl"" Type=""Edm.String"" /><Property Name=""ReportAbuseUrl"" Type=""Edm.String"" /><Property Name=""ReleaseNotes"" Type=""Edm.String"" /><Property Name=""RequireLicenseAcceptance"" Type=""Edm.Boolean"" Nullable=""false"" /><Property Name=""Summary"" Type=""Edm.String"" /><Property Name=""Tags"" Type=""Edm.String"" /><Property Name=""Title"" Type=""Edm.String"" /><Property Name=""VersionDownloadCount"" Type=""Edm.Int32"" Nullable=""false"" /><Property Name=""MinClientVersion"" Type=""Edm.String"" /><Property Name=""LastEdited"" Type=""Edm.DateTime"" /><Property Name=""LicenseUrl"" Type=""Edm.String"" /><Property Name=""LicenseNames"" Type=""Edm.String"" /><Property Name=""LicenseReportUrl"" Type=""Edm.String"" /></EntityType></Schema><Schema Namespace=""NuGetGallery"" xmlns=""http://schemas.microsoft.com/ado/2006/04/edm""><EntityContainer Name=""V2FeedContext"" m:IsDefaultEntityContainer=""true""><EntitySet Name=""Packages"" EntityType=""NuGetGallery.OData.V2FeedPackage"" /><FunctionImport Name=""Search"" ReturnType=""Collection(NuGetGallery.OData.V2FeedPackage)"" EntitySet=""Packages""><Parameter Name=""searchTerm"" Type=""Edm.String"" FixedLength=""false"" Unicode=""false"" /><Parameter Name=""targetFramework"" Type=""Edm.String"" FixedLength=""false"" Unicode=""false"" /><Parameter Name=""includePrerelease"" Type=""Edm.Boolean"" Nullable=""false"" /></FunctionImport><FunctionImport Name=""FindPackagesById"" ReturnType=""Collection(NuGetGallery.OData.V2FeedPackage)"" EntitySet=""Packages""><Parameter Name=""id"" Type=""Edm.String"" FixedLength=""false"" Unicode=""false"" /></FunctionImport><FunctionImport Name=""GetUpdates"" ReturnType=""Collection(NuGetGallery.OData.V2FeedPackage)"" EntitySet=""Packages""><Parameter Name=""packageIds"" Type=""Edm.String"" FixedLength=""false"" Unicode=""false"" /><Parameter Name=""versions"" Type=""Edm.String"" FixedLength=""false"" Unicode=""false"" /><Parameter Name=""includePrerelease"" Type=""Edm.Boolean"" Nullable=""false"" /><Parameter Name=""includeAllVersions"" Type=""Edm.Boolean"" Nullable=""false"" /><Parameter Name=""targetFrameworks"" Type=""Edm.String"" FixedLength=""false"" Unicode=""false"" /><Parameter Name=""versionConstraints"" Type=""Edm.String"" FixedLength=""false"" Unicode=""false"" /></FunctionImport></EntityContainer></Schema></edmx:DataServices></edmx:Edmx>";
			return Content(content, "text/xml", Encoding.UTF8);
		}
	}
}