namespace CF.Api.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "cf-api";
    public string Audience { get; set; } = "cf-api";

    /// <summary>HMAC-SHA256 key, at least 32 bytes. Supply it via user-secrets or the Jwt__SigningKey env var.</summary>
    public string SigningKey { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 60;

    /// <summary>
    ///     Customers granted the admin role at login. Ids rather than emails: emails aren't verified,
    ///     so anyone could register an admin email that isn't taken yet.
    /// </summary>
    public long[] AdminCustomerIds { get; set; } = [];
}
