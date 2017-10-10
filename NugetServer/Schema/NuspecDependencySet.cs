using System.Collections.Generic;
using System.Xml.Serialization;

namespace NugetServer.Schema
{
	[XmlType("group")]
	public class NuspecDependencySet
	{
		[XmlAttribute("targetFramework")]
		public string TargetFramework { get; set; }

		[XmlElement("dependency")]
		public List<NuspecDependency> Dependencies { get; set; }
	}
}