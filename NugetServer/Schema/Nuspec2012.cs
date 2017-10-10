using System.Collections.Generic;
using System.Xml.Serialization;

namespace NugetServer.Schema
{
	[XmlType("package")]
	[XmlRoot(Namespace = "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd", ElementName = "package")]
	public class Nuspec2012
	{
		[XmlElement("metadata", IsNullable = false)]
		public NuspecMetadata Metadata { get; set; }

		[XmlArray("files")]
		public List<NuspecFile> Files { get; set; }

		[XmlIgnore]
		public long Size { get; set; }

		[XmlIgnore]
		public string Hash { get; set; }

		[XmlIgnore]
		public string TargetFrameworks { get; set; }
	}
}