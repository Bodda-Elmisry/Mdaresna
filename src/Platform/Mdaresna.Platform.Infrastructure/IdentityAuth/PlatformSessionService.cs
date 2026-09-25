using System.Security.Cryptography;
using System.Text;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.IdentityAuth;

public sealed record PlatformRefreshResult(Guid AccountId, string RefreshToken);

public sealed class PlatformSessionService(IdentityDbContext identityDb)
{
    public async Task<string> CreateAsync(
        Guid accountId,
        bool rememberMe,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var token = NewToken();
        identityDb.Sessions.Add(NewSession(
            accountId,
            token,
            now,
            now.AddDays(rememberMe ? 30 : 1),
            ipAddress,
            userAgent));
        await identityDb.SaveChangesAsync(cancellationToken);
        return token;
    }

    public async Task<PlatformRefreshResult?> RotateAsync(
        string refreshToken,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return null;

        var now = DateTimeOffset.UtcNow;
        var hash = HashToken(refreshToken);
        var current = await identityDb.Sessions
            .SingleOrDefaultAsync(x => x.RefreshTokenHash == hash, cancellationToken);
        if (current is null || current.RevokedAtUtc is not null || current.ExpiresAtUtc <= now)
            return null;

        var replacementToken = NewToken();
        var replacement = NewSession(
            current.AccountId,
            replacementToken,
            now,
            current.ExpiresAtUtc,
            ipAddress,
            userAgent);
        current.LastSeenAtUtc = now;
        current.RevokedAtUtc = now;
        current.RevocationReason = "Rotated";
        current.ReplacedBySessionId = replacement.Id;
        identityDb.Sessions.Add(replacement);

        try
        {
            await identityDb.SaveChangesAsync(cancellationToken);
            return new PlatformRefreshResult(current.AccountId, replacementToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return null;
        }
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return;
        var hash = HashToken(refreshToken);
        var session = await identityDb.Sessions
            .SingleOrDefaultAsync(x => x.RefreshTokenHash == hash, cancellationToken);
        if (session is null || session.RevokedAtUtc is not null) return;
        session.RevokedAtUtc = DateTimeOffset.UtcNow;
        session.RevocationReason = "Logout";
        await identityDb.SaveChangesAsync(cancellationToken);
    }

    private static IdentitySession NewSession(
        Guid accountId,
        string token,
        DateTimeOffset now,
        DateTimeOffset expiresAtUtc,
        string? ipAddress,
        string? userAgent) => new()
    {
        Id = Guid.NewGuid(),
        AccountId = accountId,
        RefreshTokenHash = HashToken(token),
        CreatedAtUtc = now,
        ExpiresAtUtc = expiresAtUtc,
        IpAddress = Trim(ipAddress, 64),
        UserAgent = Trim(userAgent, 512)
    };

    private static string NewToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    private static string? Trim(string? value, int maximumLength) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, maximumLength)];
}
