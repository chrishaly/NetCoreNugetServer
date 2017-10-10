using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NugetServer.Models;
using NugetServer.Service;

namespace NugetServer.Controllers
{
	public class PackageController : Controller
	{
		private readonly ApplicationDataContext db;
		private readonly PackageService packageService;
		private readonly IConfiguration configuration;

		public PackageController(ApplicationDataContext db, IConfiguration configuration, PackageService packageService)
		{
			this.db = db;
			this.configuration = configuration;
			this.packageService = packageService;
		}

		[Route("FindPackagesById()")]
		public IActionResult FindPackageById()
		{
			var baseUrl = GetBaseUrl();
			var id = Request.Query["id"][0].Trim('\'');
			var query = db.Packages.Where(p => p.Identifier == id);
			var parameters = GetSearchParametersFromCurrentRequest(); // just in case
			query = packageService.QueryPackages(query, parameters);
			var packages = query.ToList();
			var count = packages.Count;
			var entries = CreateEntries(packages, null);
			var content = $@"<?xml version=""1.0"" encoding=""utf-8""?>
	<feed xml:base=""https://www.nuget.org/api/v2"" xmlns=""http://www.w3.org/2005/Atom"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns:georss=""http://www.georss.org/georss"" xmlns:gml=""http://www.opengis.net/gml""><m:count>{count}</m:count><id>http://schemas.datacontract.org/2004/07/</id><title /><updated>2017-06-05T01:45:25Z</updated><link rel=""self"" href=""{baseUrl}/Packages"" />{entries}</feed>";
			return Content(content, "text/xml");
		}

		[Route("Packages()")]
		public IActionResult Packages()
		{
			return Search();
		}

		[Route("Search()")]
		public IActionResult Search()
		{
			var baseUrl = GetBaseUrl();
			var parameters = GetSearchParametersFromCurrentRequest();
			var query =packageService. QueryPackages(parameters);
			//var count = query.Count();
			var packages = query.ToList();
			packages = packages.GroupBy(it => it.Identifier).Select(g => g.First()).ToList();
			// I know this is inefficient, yet I put these togather under 6 hours (4 hours actually).
			packages = packages.Skip(parameters.Skip).Take(parameters.Take).ToList();
			var entries = CreateEntries(packages, parameters.SelectedFields);
			var content = $@"<?xml version=""1.0"" encoding=""utf-8""?>
	<feed xml:base=""https://www.nuget.org/api/v2"" xmlns=""http://www.w3.org/2005/Atom"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns:georss=""http://www.georss.org/georss"" xmlns:gml=""http://www.opengis.net/gml""><m:count>0</m:count><id>http://schemas.datacontract.org/2004/07/</id><title /><updated>{DateTime.UtcNow}</updated><link rel=""self"" href=""{baseUrl}/Packages"" />{entries}</feed>";

			return Content(content, "text/xml");
		}

		[Route("Package/{id}/{version}")]
		public IActionResult Package(string id, string version)
		{
			var packageDir = configuration.GetValue<string>("Package_Directory");
			var packageName = $"{id}.{version}.nupkg";
			return File(System.IO.File.OpenRead(Path.Combine(packageDir, packageName)), "application/zip", packageName);
		}

		private string CreateEntries(IEnumerable<Package> packages, string[] selectedFields)
		{
			if (selectedFields == null)
			{
				return string.Concat(packages.Select(CreateEntry));
			}

			var baseUrl = GetBaseUrl();
			var properties = typeof(Package).GetProperties().Where(p => selectedFields.Contains(p.Name));
			var builder = new System.Text.StringBuilder();
			builder.Append($@"<entry>
		<id>{baseUrl}/Packages(Id='{{Identifier}}',Version='{{Version}}')</id>
		<category term=""NuGetGallery.OData.V2FeedPackage"" scheme=""http://schemas.microsoft.com/ado/2007/08/dataservices/scheme"" />
		<link rel=""edit"" href=""{baseUrl}/Packages(Id='{{Identifier}}',Version='{{Version}}')"" />
		<link rel=""self"" href=""{baseUrl}/Packages(Id='{{Identifier}}',Version='{{Version}}')"" />
		<title type=""text"">{{Identifier}}</title>
		<updated>2017-04-02T13:37:50Z</updated>
		<author>
			<name>{{Authors}}</name>
		</author>
		<content type=""application/zip"" src=""{baseUrl}/Package/{{Identifier}}/{{Version}}"" />
		<m:properties>");
			var accessors = new List<Tuple<string, PropertyInfo>>();
			accessors.Add(Tuple.Create("Identifier", typeof(Package).GetProperty("Identifier")));
			accessors.Add(Tuple.Create("Authors", typeof(Package).GetProperty("Authors")));
			accessors.Add(Tuple.Create("Version", typeof(Package).GetProperty("Version")));
			foreach (var property in properties)
			{
				if (property.Name == "Id")
				{
					builder.Append(@"<d:Id>{Identifier}</d:Id>");
					continue;
				}
				accessors.Add(Tuple.Create(property.Name, property));
				if (property.PropertyType == typeof(string))
				{
					builder.Append($@"<d:{property.Name}>{{{property.Name}}}</d:{property.Name}>");
				}
				else
				{
					builder.Append($@"<d:{property.Name} m:type=""Edm.{property.PropertyType.Name.Replace("System", string.Empty)}"">{{{property.Name}}}</d:{property.Name}>");
				}
			}

			builder.Append("</m:properties></entry>");

			var formatString = builder.ToString();

			Func<Package, string> entryFunc = (package) => { var resultString = formatString; foreach (var accessor in accessors) { resultString = resultString.Replace($"{{{accessor.Item1}}}", accessor.Item2.GetValue(package).ToString()); } return resultString; };

			return string.Concat(packages.Select(package => CreateEntry(package, entryFunc)));
		}

		private string CreateEntry(Package package, Func<Package, string> entryFunc)
		{
			return entryFunc(package);
		}

		private string CreateEntry(Package package)
		{
			var baseUrl = GetBaseUrl();
			return $@"<entry>
		<id>{baseUrl}/Packages(Id='{package.Identifier}',Version='{package.Version}')</id>
		<category term=""NuGetGallery.OData.V2FeedPackage"" scheme=""http://schemas.microsoft.com/ado/2007/08/dataservices/scheme"" />
		<link rel=""edit"" href=""{baseUrl}/Packages(Id='{package.Identifier}',Version='{package.Version}')"" />
		<link rel=""self"" href=""{baseUrl}/Packages(Id='{package.Identifier}',Version='{package.Version}')"" />
		<title type=""text"">{package.Identifier}</title>
		<updated>2017-04-02T13:37:50Z</updated>
		<author>
			<name>{package.Authors}</name>
		</author>
		<content type=""application/zip"" src=""{baseUrl}/Package/{package.Identifier}/{package.Version}"" />
		<m:properties>
			<d:Id>{package.Identifier}</d:Id>
			<d:Version>{package.Version}</d:Version>
			<d:NormalizedVersion>{package.Version}</d:NormalizedVersion>
			<d:Authors>{package.Authors}</d:Authors>
			<d:Copyright>{package.Copyright}</d:Copyright>
			<d:Created m:type=""Edm.DateTime"">{package.Created}</d:Created>
			<d:Dependencies>{package.Dependencies}</d:Dependencies>
			<d:Description>{package.Description}</d:Description>
			<d:DownloadCount m:type=""Edm.Int32"">{package.DownloadCount}</d:DownloadCount>
			<d:GalleryDetailsUrl>{baseUrl}/api/v2/package/{package.Identifier}/{package.Version}</d:GalleryDetailsUrl>
			<!--d:IconUrl>http://www.newtonsoft.com/content/images/nugeticon.png</d:IconUrl-->
			<d:IsLatestVersion m:type=""Edm.Boolean"">{package.IsLatestVersion}</d:IsLatestVersion>
			<d:IsAbsoluteLatestVersion m:type=""Edm.Boolean"">{package.IsAbsoluteLatestVersion}</d:IsAbsoluteLatestVersion>
			<d:IsPrerelease m:type=""Edm.Boolean"">{package.IsPrerelease}</d:IsPrerelease>
			<d:Language>{package.Language}</d:Language>
			<d:LastUpdated m:type=""Edm.DateTime"">{package.LastUpdated}</d:LastUpdated>
			<d:Published m:type=""Edm.DateTime"">{package.Published}</d:Published>
			<d:PackageHash>{package.PackageHash}</d:PackageHash>
			<d:PackageHashAlgorithm>{package.PackageHashAlgorithm}</d:PackageHashAlgorithm>
			<d:PackageSize m:type=""Edm.Int64"">{package.PackageSize}</d:PackageSize>
			<d:ProjectUrl>{package.ProjectUrl}</d:ProjectUrl>
			<d:ReportAbuseUrl>{package.ReportAbuseUrl}</d:ReportAbuseUrl>
			<d:ReleaseNotes>{package.ReleaseNotes}</d:ReleaseNotes>
			<d:RequireLicenseAcceptance m:type=""Edm.Boolean"">{package.RequireLicenseAcceptance}</d:RequireLicenseAcceptance>
			<d:Summary>{package.Summary}</d:Summary>
			<d:Tags>{package.Tags}</d:Tags>
			<d:Title>{package.Title}</d:Title>
			<d:VersionDownloadCount m:type=""Edm.Int32"">{package.VersionDownloadCount}</d:VersionDownloadCount>
			<d:MinClientVersion>{package.MinClientVersion}</d:MinClientVersion>
			<d:LastEdited>{package.LastEdited}</d:LastEdited>
			<d:LicenseUrl>{package.LicenseUrl}</d:LicenseUrl>
			<d:LicenseNames>{package.LicenseNames}</d:LicenseNames>
			<d:LicenseReportUrl>{package.LicenseReportUrl}</d:LicenseReportUrl>
		</m:properties>
	</entry>";
		}

		private string GetBaseUrl()
		{
			return $"{Request.Scheme.ToLower()}://{Request.Host}";
		}

		private QueryParameters GetSearchParametersFromCurrentRequest()
		{
			var parameters = new QueryParameters();
			if (Request.Query.ContainsKey("$filter") && Request.Query["$filter"].Count > 0)
			{
				var filter = Request.Query["$filter"][0];
				if (filter.StartsWith("'") && filter.EndsWith("'"))
				{
					parameters.SearchTerm = filter.Trim('\'');
				}
				else
				{
					if (filter == "IsLatestVersion")
					{
						parameters.IsLatestVersion = true;
					}
					else if (filter == "IsAbsoluteLatestVersion")
					{
						parameters.IsAbsoluteLatestVersion = true;
					}
				}

				var searchTerm = Request.Query["searchTerm"].FirstOrDefault();
				if (searchTerm != null && searchTerm.StartsWith("'") && searchTerm.EndsWith("'"))
				{
					parameters.SearchTerm = searchTerm.Trim('\'');
				}

				var includePrerelease = Request.Query["includePrerelease"].FirstOrDefault();
				parameters.IncludePrerelease = includePrerelease == "true";
			}

			if (Request.Query.ContainsKey("targetFramework") && Request.Query["targetFramework"].Count > 0)
			{
				parameters.TargetFramework = Request.Query["targetFramework"][0].Trim('\'');
			}

			if (Request.Query.ContainsKey("$orderBy") && Request.Query["$orderBy"].Count > 0)
			{
				var orderBy = Request.Query["$orderBy"][0].Split(' ');
				parameters.OrderBy = orderBy[0];
				if (orderBy.Length > 1 && orderBy[1] == "desc")
				{
					parameters.OrderByDescending = true;
				}
			}

			if (Request.Query.ContainsKey("$skip") && Request.Query["$skip"].Count > 0)
			{
				parameters.Skip = Convert.ToInt32(Request.Query["$skip"][0]);
			}

			if (Request.Query.ContainsKey("$top") && Request.Query["$top"].Count > 0)
			{
				parameters.Take = Convert.ToInt32(Request.Query["$top"][0]);
			}
			else
			{
				parameters.Take = 5;
			}

			if (Request.Query.ContainsKey("$select") && Request.Query["$select"].Count > 0)
			{
				parameters.SelectedFields = Request.Query["$select"][0].Split(',');
			}

			return parameters;
		}
	}
}