using System.Linq;
using NugetServer.Controllers;
using NugetServer.Models;

namespace NugetServer.Service
{
	public class PackageService
	{
		private readonly ApplicationDataContext _db;

		public PackageService(ApplicationDataContext db)
		{
			_db = db;
		}

		public IQueryable<Package> QueryPackages(QueryParameters parameters)
		{
			var query = _db.Packages.AsQueryable();
			return QueryPackages(query, parameters);
		}

		public IQueryable<Package> QueryPackages(IQueryable<Package> query, QueryParameters parameters)
		{
			//query = query.GroupBy(it => it.Identifier).Select(g => g.First());
			if (!string.IsNullOrEmpty(parameters.SearchTerm))
			{
				var searchTerm = parameters.SearchTerm;
				//query = StartsWithSearchTerm(query, searchTerm)
				//	.Union(ContainsSearchTerm(query, searchTerm))
				//	.Union(TitleStartsWithSearchTerm(query, searchTerm))
				//	.Union(TitleContainsSearchTerm(query, searchTerm))
				//	.Union(TagsContainsSearchTerm(query, searchTerm))
				//	.Union(DescriptionContainsSearchTerm(query, searchTerm));

				query = query.Where(it => it.Identifier.Contains(searchTerm)
					|| it.Title.Contains(searchTerm)
					|| it.Tags.Contains(searchTerm)
					|| it.Description.Contains(searchTerm));
			}

			if (!string.IsNullOrEmpty(parameters.TargetFramework))
			{
				query = query.Where(p => p.TargetFrameworks.Contains(parameters.TargetFramework));
			}

			if (!parameters.IncludePrerelease)
			{
				query = query.Where(p => !p.Version.Contains("beta") && !p.Version.Contains("alpha"));
			}

			if (!string.IsNullOrEmpty(parameters.OrderBy))
			{
				// it is too late, like 3 am late.
				// todo: implement sorting, or just leave it, you will end up using odata eventually.
			}

			if (parameters.IsLatestVersion)
			{
				// allora
			}

			if (parameters.IsAbsoluteLatestVersion)
			{
				// is latest version (for real?)
			}

			return query;
		}

	}
}
