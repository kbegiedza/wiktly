namespace Wiktly.Web.Constants;

public static class AuthenticationSchemas
{
    /// <summary>
    /// This scheme allows either cookie-based authentication (for web clients) or token-based authentication (for API clients).
    /// </summary>
    public static class CookieOrTokenScheme
    {
        public const string Key = "WebCookieOrApiTokenScheme";
        public const string Name = "Cookie or Token";
    }
}

