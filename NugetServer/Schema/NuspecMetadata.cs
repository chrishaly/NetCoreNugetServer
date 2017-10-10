using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace NugetServer.Schema
{
	[XmlType("metadata")]
	public class NuspecMetadata
	{
		[XmlAttribute("minClientVersion")]
		public string MinClientVersion { get; set; }

		[XmlElement("id")]
		public string Id { get; set; }

		[XmlElement("version")]
		public string Version { get; set; }

		[XmlElement("title")]
		public string Title { get; set; }

		[XmlElement("authors")]
		public string Authors { get; set; }

		[XmlElement("owners")]
		public string Owners { get; set; }

		[XmlElement("licenseUrl")]
		public string LicenseUrl { get; set; }

		[XmlElement("projectUrl")]
		public string ProjectUrl { get; set; }

		[XmlElement("iconUrl")]
		public string IconUrl { get; set; }

		[XmlElement("requireLicenseAcceptance")]
		public bool RequireLicenseAcceptance { get; set; }

		[XmlElement("developmentDependency")]
		public string DevelopmentDependency { get; set; }

		[XmlElement("description")]
		public string Description { get; set; }

		[XmlElement("summary")]
		public string Summary { get; set; }

		[XmlElement("releaseNotes")]
		public string ReleaseNotes { get; set; }

		[XmlElement("copyright")]
		public string Copyright { get; set; }

		[XmlElement("language")]
		public string Language { get; set; }

		[XmlElement("tags")]
		public string Tags { get; set; }

		[XmlArray("dependencies")]
		[XmlArrayItem("group", typeof(NuspecDependencySet))]
		[XmlArrayItem("dependency", typeof(NuspecDependency))]
		public List<object> DependencySerialization { get; set; }

		[XmlIgnore]
		public IEnumerable<NuspecDependencySet> DependencySets
		{
			get
			{
				if (DependencySerialization[0] is NuspecDependencySet)
				{
					return DependencySerialization.Cast<NuspecDependencySet>();
				}
				else if (DependencySerialization[0] is NuspecDependency)
				{
					return new List<NuspecDependencySet> { new NuspecDependencySet { Dependencies = DependencySerialization.Cast<NuspecDependency>().ToList() } };
				}
				else
				{
					throw new InvalidOperationException();
				}
			}
		}
	}
}