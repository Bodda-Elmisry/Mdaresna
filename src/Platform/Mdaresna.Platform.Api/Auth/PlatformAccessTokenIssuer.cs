using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Mdaresna.Platform.Infrastructure.IdentityAuth;
using Microsoft.IdentityModel.Tokens;

namespace Mdaresna.Platform.Api.Auth;

public sealed record PlatformAccessToken(string Token, int ExpiresInSeconds, DateTimeOffset ExpiresAtUtc);

public interface IPlatformAccessTokenIssuer
{
    PlatformAccessToken Issue(PlatformLoginResult login);
}

internal sealed class PlatformAccessTokenIssuer(PlatformJwtOptions options) : IPlatformAccessTokenIssuer
{
    public PlatformAccessToken Issue(PlatformLoginResult login)
    {
        ArgumentNullException.ThrowIfNull(login);

        if (login.AccountId == Guid.Empty || string.IsNullOrWhiteSpace(login.SecurityStamp))
        {
            throw new ArgumentException("A valid Platform login is required.", nameof(login));
        }

        var now = DateTimeOffset.UtcNow;
        var expiry = now.AddMinutes(options.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, login.AccountId.ToString("D")),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(PlatformTokenClaims.Purpose, PlatformTokenClaims.TokenPurpose),
            new(PlatformTokenClaims.SecurityStamp, login.SecurityStamp)
        };
        if (!string.IsNullOrWhiteSpace(login.DisplayName))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Name, login.DisplayName));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = options.Issuer,
            Audience = options.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiry.UtcDateTime,
            SigningCredentials = new SigningCredentials(
                options.CreateSecurityKey(), SecurityAlgorithms.HmacSha256)
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);
        return new PlatformAccessToken(
            handler.WriteToken(token),
            checked(options.AccessTokenMinutes * 60),
            expiry);
    }
}
