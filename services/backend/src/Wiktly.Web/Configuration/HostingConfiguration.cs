namespace Wiktly.Web.Configuration;

public class HostingConfiguration
{
    /// <summary>
    /// Gets or sets the allowed origins.
    /// </summary>
    public required string[] AllowedOrigins { get; set; }

    /// <summary>
    /// Gets or sets the URL of the API.
    /// </summary>
    public required string? Url { get; set; }

    public required bool MigrateOnStartup { get; set; } = false;
}