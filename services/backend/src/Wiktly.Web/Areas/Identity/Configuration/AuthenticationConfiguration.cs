namespace Wiktly.Web.Areas.Identity.Configuration;

public sealed record AuthenticationConfiguration
{
    /// <summary>
    /// Encryption key for symmetric encryption of tokens.<br />
    /// Encoded as Base64 string.
    /// </summary>
    public required string EncryptionKey { get; init; } = string.Empty;

    /// <summary>
    /// Connection string to the database.
    /// </summary>
    public required string ConnectionString { get; init; } = string.Empty;

    /// <summary>
    /// Collection of default client applications to be created on startup if they do not exist.
    /// </summary>
    public required Application[] DefaultApplications { get; init; } = [];

    /// <summary>
    /// Represents a client application with its credentials.
    /// </summary>
    public record Application
    {
        /// <summary>
        /// Name of the client application.
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Identifier of the client application.
        /// </summary>
        public required string ClientId { get; init; } = string.Empty;

        /// <summary>
        /// Secret of the client application.
        /// </summary>
        public required string ClientSecret { get; init; } = string.Empty;
    }
}
