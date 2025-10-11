namespace Wiktly.Web.Areas.Identity.Configuration;

public sealed class AuthenticationConfiguration
{
    /// <summary>
    /// Encryption key for symmetric encryption of tokens.<br />
    /// Encoded as Base64 string.
    /// </summary>
    public string EncryptionKey { get; set; } = string.Empty;

    public string ConnectionString { get; set; } = string.Empty;
}