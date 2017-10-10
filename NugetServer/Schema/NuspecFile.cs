using System.Xml.Serialization;

namespace NugetServer.Schema
{
	[XmlType("file")]
	public class NuspecFile
	{
		[XmlAttribute("src")]
		public string Source { get; set; }

		[XmlAttribute("target")]
		public string Target { get; set; }

		[XmlAttribute("exclude")]
		public string Exclude { get; set; }
	}
}