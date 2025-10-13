using Asp.Versioning;

namespace Wiktly.Web.Configuration;

public class VersioningConfiguration
{
    public required List<ApiVersion> ApiVersions { get; set; }
}