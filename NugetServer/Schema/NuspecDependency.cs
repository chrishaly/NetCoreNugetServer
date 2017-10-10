using System.Xml.Serialization;

namespace NugetServer.Schema
{
	[XmlType("dependency")]
	public class NuspecDependency
	{
		[XmlAttribute("id")]
		public string Id { get; set; }

		[XmlAttribute("version")]
		public string Version { get; set; }

		[XmlAttribute("include")]
		public string Include { get; set; }

		[XmlAttribute("exclude")]
		public string Exclude { get; set; }
	}
}