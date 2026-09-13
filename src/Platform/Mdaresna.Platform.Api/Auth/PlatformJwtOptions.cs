using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Mdaresna.Platform.Api.Auth;

internal sealed class PlatformJwtOptions
{
    private PlatformJwtOptions(
        string issuer,
        string audience,
        byte[] signingKey,
        int accessTokenMinutes)
    {
        Issuer = issuer;
        Audience = audience;
        SigningKey = signingKey;
        AccessTokenMinutes = accessTokenMinutes;
    }

    public string Issuer { get; }
    public string Audience { get; }
    public byte[] SigningKey { get; }
    public int AccessTokenMinutes { get; }

    public static PlatformJwtOptions FromConfiguration(IConfiguration configuration)
    {
        var issuer = configuration["PlatformAuth:Issuer"]?.Trim();
        var audience = configuration["PlatformAuth:Audience"]?.Trim();
        var secret = configuration["PlatformAuth:SigningKey"];

        if (string.IsNullOrWhiteSpace(issuer) ||
            string.IsNullOrWhiteSpace(audience) ||
            string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException(
                "PlatformAuth issuer, audience, and signing key are required.");
        }

        var signingKey = Encoding.UTF8.GetBytes(secret);
        if (signingKey.Length < 32)
        {
            throw new InvalidOperationException(
                "PlatformAuth signing key must be at least 32 UTF-8 bytes.");
        }

        var accessTokenMinutes = 15;
        var configuredLifetime = configuration["PlatformAuth:AccessTokenMinutes"];
        if (configuredLifetime is not null &&
            !int.TryParse(configuredLifetime, out accessTokenMinutes))
        {
            throw new InvalidOperationException(
                "PlatformAuth access token lifetime must be an integer number of minutes.");
        }

        if (accessTokenMinutes is < 1 or > 15)
        {
            throw new InvalidOperationException(
                "PlatformAuth access token lifetime must be between 1 and 15 minutes.");
        }

        return new PlatformJwtOptions(issuer, audience, signingKey, accessTokenMinutes);
    }

    public SymmetricSecurityKey CreateSecurityKey() => new(SigningKey);
}
