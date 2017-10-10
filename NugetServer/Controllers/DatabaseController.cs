using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NugetServer.Models;
using NugetServer.Schema;

namespace NugetServer.Controllers
{
	[Produces("application/json")]
	public class DatabaseController : Controller
	{
		private ApplicationDataContext db;
		private IConfiguration configuration;

		public DatabaseController(ApplicationDataContext db, IConfiguration configuration)
		{
			this.db = db;
			this.configuration = configuration;
		}

		[Route("BuildIndex")]
		public IndexResult BuildIndex()
		{
			var specs = RetrieveSpecs();

			// todo: semantic versioning & version comparison (write that version class!)
			// todo: use semantic versioning to compare different versions of same package, and set islatest... properties.

			var added = new List<Nuspec>();
			var updated = new List<Nuspec>();
			foreach (var spec in specs)
			{
				db.Database.EnsureCreated();
				var package = db.Packages.FirstOrDefault(p => p.Identifier == spec.Metadata.Id && p.Version == spec.Metadata.Version);
				if (package == null)
				{
					added.Add(spec);
					AddNewPackage(spec);
				}
				else
				{
					updated.Add(spec);
					UpdatePackage(package, spec);
				}
			}

			var remainingKeys = added.Union(updated).Select(spec => $"{spec.Metadata.Id}:{spec.Metadata.Version}").ToArray();

			var deletedPackages = db.Packages.Select(p => new { Package = p, Key = $"{p.Identifier}:{p.Version}" }).Where(p => !remainingKeys.Contains(p.Key)).Select(p => p.Package).ToList();

			var deleted = deletedPackages.Select(dp => $"{dp.Identifier}:{dp.Version}").ToArray();
			db.RemoveRange(deletedPackages);

			db.SaveChanges();
			return new IndexResult
			{
				Added = added.Select(p => $"{p.Metadata.Id}:{p.Metadata.Version}").ToArray(),
				Updated = updated.Select(p => $"{p.Metadata.Id}:{p.Metadata.Version}").ToArray(),
				Deleted = deleted
			};
		}

		private void UpdatePackage(Package package, Nuspec spec)
		{
			var baseUrl = $"{Request.Scheme.ToLower()}://{Request.Host}";

			package.Identifier = spec.Metadata.Id;
			package.Description = spec.Metadata.Description;
			package.Dependencies = CreateDependencyString(spec.Metadata.DependencySets);
			package.MinClientVersion = spec.Metadata.MinClientVersion;
			package.Version = spec.Metadata.Version;
			package.IsPrerelease = spec.Metadata.Version.Contains("beta");
			package.Title = spec.Metadata.Title;
			package.Authors = spec.Metadata.Authors;
			package.Owners = spec.Metadata.Owners;
			package.IconUrl = spec.Metadata.IconUrl;
			package.LicenseUrl = spec.Metadata.LicenseUrl;
			package.ProjectUrl = spec.Metadata.ProjectUrl;
			package.RequireLicenseAcceptance = spec.Metadata.RequireLicenseAcceptance;
			package.DevelopmentDependency = spec.Metadata.DevelopmentDependency;
			package.Summary = spec.Metadata.Summary;
			package.ReleaseNotes = spec.Metadata.ReleaseNotes;
			package.Tags = spec.Metadata.Tags;
			package.PackageSize = spec.Size;
			package.PackageHash = spec.Hash;
			package.PackageHashAlgorithm = "SHA512";
			package.GalleryDetailsUrl = $"{baseUrl}/Package/{spec.Metadata.Id}/{spec.Metadata.Version}";
			package.TargetFrameworks = spec.TargetFrameworks;
		}

		private void AddNewPackage(Nuspec spec)
		{
			var baseUrl = $"{Request.Scheme.ToLower()}://{Request.Host}";

			db.Packages.Add(new Package
			{
				Identifier = spec.Metadata.Id,
				Description = spec.Metadata.Description,
				Dependencies = CreateDependencyString(spec.Metadata.DependencySets),
				MinClientVersion = spec.Metadata.MinClientVersion,
				Version = spec.Metadata.Version,
				IsPrerelease = spec.Metadata.Version.Contains("beta"),
				Title = spec.Metadata.Title,
				Authors = spec.Metadata.Authors,
				Owners = spec.Metadata.Owners,
				IconUrl = spec.Metadata.IconUrl,
				LicenseUrl = spec.Metadata.LicenseUrl,
				ProjectUrl = spec.Metadata.ProjectUrl,
				RequireLicenseAcceptance = spec.Metadata.RequireLicenseAcceptance,
				DevelopmentDependency = spec.Metadata.DevelopmentDependency,
				Summary = spec.Metadata.Summary,
				ReleaseNotes = spec.Metadata.ReleaseNotes,
				Tags = spec.Metadata.Tags,
				PackageSize = spec.Size,
				PackageHash = spec.Hash,
				PackageHashAlgorithm = "SHA512",
				GalleryDetailsUrl = $"{baseUrl}/Package/{spec.Metadata.Id}/{spec.Metadata.Version}",
				TargetFrameworks = spec.TargetFrameworks
			});
		}

		private List<Nuspec> RetrieveSpecs()
		{
			var packageFileNames = GetPackageFileNames();
			var serializer = CreateSerializer();
			var serializer2012 = new XmlSerializer(typeof(Nuspec2012));
			var hashAlgorithm = SHA512.Create();
			var specs = new List<Nuspec>();
			foreach (var packageFileName in packageFileNames)
			{
				try
				{
					long fileSize;
					byte[] fileHashBytes;
					using (var archiveFileStream = System.IO.File.Open(packageFileName, FileMode.Open))
					{
						fileSize = archiveFileStream.Length;
						fileHashBytes = hashAlgorithm.ComputeHash(archiveFileStream);
					}

					using (var archive = ZipFile.Open(packageFileName, ZipArchiveMode.Read))
					{
						var targetFrameworks = string.Join(",", archive.Entries.Where(e => e.FullName.Contains("lib/")).Select(e => e.FullName.Split('/')[1]));
						var nuspecFile = archive.Entries.First(e => e.Name.EndsWith(".nuspec", StringComparison.Ordinal));
						try
						{
							using (var stream = nuspecFile.Open())
						{
							if (serializer.Deserialize(stream) is Nuspec spec)
							{
								spec.Size = fileSize;
								spec.Hash = Convert.ToBase64String(fileHashBytes);
								spec.TargetFrameworks = targetFrameworks;
								specs.Add(spec);
							}
						}
						}
						catch (Exception ex)
						{
							using (var stream = nuspecFile.Open())
							{
								if (serializer2012.Deserialize(stream) is Nuspec2012 spec2012)
								{
									var spec = new Nuspec();
									Mapper.Map(spec2012, spec);

									spec.Size = fileSize;
									spec.Hash = Convert.ToBase64String(fileHashBytes);
									spec.TargetFrameworks = targetFrameworks;
									specs.Add(spec);
								}
							}
							//throw;
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(packageFileName);
					//Console.WriteLine(ex);
					//Console.WriteLine("-------------------------");
				}

			}

			return specs;
		}

		private IEnumerable<string> GetPackageFileNames()
		{
			return Directory.EnumerateFiles(configuration.GetValue<string>("Package_Directory"), "*.nupkg");
		}

		private static XmlSerializer CreateSerializer()
		{
			return new XmlSerializer(typeof(Nuspec));
		}

		private static string FrameworkNameToMoniker(string frameworkName)
		{
			const string netframeworkFrameworkName = ".NETFramework";
			const string netframeworkMonikerPrefix = "net";
			const string netcoreAppFrameworkName = ".NETCoreApp";
			const string netcoreAppFrameworkMoniker = "netcoreapp";
			const string netstandardFrameworkName = ".NETStandard";
			const string netstandardFrameworkMoniker = "netstandard";
			if (frameworkName.Contains(netframeworkFrameworkName))
			{
				var versionPart = frameworkName.Replace(netframeworkFrameworkName, string.Empty);
				return $"{netframeworkMonikerPrefix}{versionPart.Replace(".", string.Empty)}";
			}

			if (frameworkName.Contains(netcoreAppFrameworkName))
			{
				var versionPart = frameworkName.Replace(netcoreAppFrameworkName, string.Empty);
				return $"{netcoreAppFrameworkMoniker}{versionPart}";
			}

			if (frameworkName.Contains(netstandardFrameworkName))
			{
				var versionPart = frameworkName.Replace(netstandardFrameworkName, string.Empty);
				return $"{netstandardFrameworkMoniker}{versionPart}";
			}

			else
			{
				throw new ArgumentOutOfRangeException(nameof(frameworkName));
			}
		}

		private static string CreateDependencyString(IEnumerable<NuspecDependencySet> sets)
		{
			return string.Join("|", sets.Select(CreateDependencyString));
		}

		private static string CreateDependencyString(NuspecDependencySet set)
		{
			if (set.Dependencies == null || set.Dependencies.Count == 0)
			{
				if (set.TargetFramework != null)
				{
					return $"::{FrameworkNameToMoniker(set.TargetFramework)}";
				}
			}

			if (set.TargetFramework == null)
			{
				return string.Join("|", set.Dependencies.Select(d => string.IsNullOrEmpty(d.Version) ? d.Id : $"{d.Id}:[{d.Version}, )"));
			}

			//return string.Join("|", set.Dependencies.Select(d => $"{d.Id}:[{d.Version}, ):{FrameworkNameToMoniker(set.TargetFramework)}"));
			return string.Join("|", set.Dependencies.Select(d => $"{d.Id}:{d.Version}:{FrameworkNameToMoniker(set.TargetFramework)}"));
		}
	}
}