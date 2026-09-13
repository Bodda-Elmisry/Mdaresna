using System.ComponentModel.DataAnnotations;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Messaging;

public sealed class PlatformSmsProviderValues
{
    public string ProviderUserName { get; init; } = string.Empty;
    public string? ProviderPassword { get; init; }
    public string SenderName { get; init; } = string.Empty;
    public string ApiUrlTemplate { get; init; } = string.Empty;
    public int MessageCharactersLength { get; init; }
    public int Priority { get; init; }
    public string SuccessResponsePrefix { get; init; } = string.Empty;
}

public sealed record PlatformSmsProviderReadModel(
    Guid Id,
    string ProviderUserName,
    string SenderName,
    string ApiUrlTemplate,
    int MessageCharactersLength,
    int Priority,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string Version,
    string SuccessResponsePrefix,
    bool HasPassword);

public sealed record PlatformSmsProviderPage(
    IReadOnlyList<PlatformSmsProviderReadModel> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);

/// <summary>
/// Platform-owned provider management. Passwords are write-only at the API
/// boundary and are encrypted before EF sees them.
/// </summary>
public sealed class PlatformSmsProviderService(
    PlatformDbContext db,
    PlatformSmsSecretProtector secrets)
{
    public async Task<PlatformSmsProviderPage> ListAsync(
        bool? isActive,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        if (pageNumber < 1 || pageSize is < 1 or > 100 ||
            ((long)pageNumber - 1) * pageSize > int.MaxValue)
        {
            throw new ValidationException("Invalid provider listing request.");
        }

        var query = db.SmsProviders.AsNoTracking().Where(x => !x.IsDeleted);
        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        var count = await query.CountAsync(ct);
        var entities = await query.OrderBy(x => x.Priority)
            .ThenBy(x => x.CreatedAtUtc)
            .ThenBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return new PlatformSmsProviderPage(
            entities.Select(ToReadModel).ToArray(), count, pageNumber, pageSize);
    }

    public async Task<PlatformSmsProviderReadModel> GetAsync(Guid id, CancellationToken ct = default) =>
        ToReadModel(await FindAsync(id, ct));

    public async Task<PlatformSmsProviderReadModel> CreateAsync(
        PlatformSmsProviderValues values,
        Guid actorAccountId,
        string? correlationId,
        CancellationToken ct = default)
    {
        Validate(values, requirePassword: true);
        var now = DateTimeOffset.UtcNow;
        var provider = new PlatformSmsProvider
        {
            Id = Guid.NewGuid(),
            ProviderUserName = values.ProviderUserName.Trim(),
            EncryptedPassword = secrets.Protect(values.ProviderPassword!),
            SenderName = values.SenderName.Trim(),
            ApiUrlTemplate = values.ApiUrlTemplate.Trim(),
            MessageCharactersLength = values.MessageCharactersLength,
            Priority = values.Priority,
            SuccessResponsePrefix = values.SuccessResponsePrefix.Trim(),
            IsActive = false,
            IsDeleted = false,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        db.SmsProviders.Add(provider);
        AddAudit(actorAccountId, provider.Id, "platform.sms_provider.created", correlationId, now);
        await db.SaveChangesAsync(ct);
        return ToReadModel(provider);
    }

    public async Task<PlatformSmsProviderReadModel> UpdateAsync(
        Guid id,
        string expectedVersion,
        PlatformSmsProviderValues values,
        Guid actorAccountId,
        string? correlationId,
        CancellationToken ct = default)
    {
        Validate(values, requirePassword: false);
        var provider = await FindAsync(id, ct);
        RequireVersion(provider, expectedVersion);
        provider.ProviderUserName = values.ProviderUserName.Trim();
        if (values.ProviderPassword is not null)
        {
            if (string.IsNullOrWhiteSpace(values.ProviderPassword) ||
                values.ProviderPassword.Length > 300)
            {
                throw new ValidationException("Provider password is invalid.");
            }

            provider.EncryptedPassword = secrets.Protect(values.ProviderPassword);
        }

        provider.SenderName = values.SenderName.Trim();
        provider.ApiUrlTemplate = values.ApiUrlTemplate.Trim();
        provider.MessageCharactersLength = values.MessageCharactersLength;
        provider.Priority = values.Priority;
        provider.SuccessResponsePrefix = values.SuccessResponsePrefix.Trim();
        provider.UpdatedAtUtc = DateTimeOffset.UtcNow;
        AddAudit(actorAccountId, id, "platform.sms_provider.updated", correlationId, provider.UpdatedAtUtc);
        await db.SaveChangesAsync(ct);
        return ToReadModel(provider);
    }

    public async Task<PlatformSmsProviderReadModel> SetActiveAsync(
        Guid id,
        string expectedVersion,
        bool active,
        Guid actorAccountId,
        string? correlationId,
        CancellationToken ct = default)
    {
        var provider = await FindAsync(id, ct);
        RequireVersion(provider, expectedVersion);
        if (provider.IsActive != active)
        {
            provider.IsActive = active;
            provider.UpdatedAtUtc = DateTimeOffset.UtcNow;
            AddAudit(actorAccountId, id,
                active ? "platform.sms_provider.activated" : "platform.sms_provider.deactivated",
                correlationId, provider.UpdatedAtUtc);
            await db.SaveChangesAsync(ct);
        }

        return ToReadModel(provider);
    }

    public async Task DeleteAsync(
        Guid id,
        string expectedVersion,
        Guid actorAccountId,
        string? correlationId,
        CancellationToken ct = default)
    {
        var provider = await FindAsync(id, ct);
        RequireVersion(provider, expectedVersion);
        provider.IsDeleted = true;
        provider.IsActive = false;
        provider.UpdatedAtUtc = DateTimeOffset.UtcNow;
        AddAudit(actorAccountId, id, "platform.sms_provider.deleted", correlationId, provider.UpdatedAtUtc);
        await db.SaveChangesAsync(ct);
    }

    private async Task<PlatformSmsProvider> FindAsync(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty)
        {
            throw new ValidationException("Provider ID is required.");
        }

        return await db.SmsProviders.SingleOrDefaultAsync(
                   x => x.Id == id && !x.IsDeleted, ct)
               ?? throw new PlatformResourceNotFoundException(
                   "sms_provider.not_found", "SMS provider was not found.");
    }

    private static void RequireVersion(PlatformSmsProvider provider, string expectedVersion)
    {
        byte[] supplied;
        try
        {
            supplied = Convert.FromBase64String(expectedVersion);
        }
        catch (Exception exception) when (exception is ArgumentNullException or FormatException)
        {
            throw new ValidationException("A valid version is required.");
        }

        if (!supplied.AsSpan().SequenceEqual(provider.RowVersion))
        {
            throw new PlatformConflictException(
                "sms_provider.version_conflict", "The provider changed since it was last read.");
        }
    }

    private static void Validate(PlatformSmsProviderValues values, bool requirePassword)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (string.IsNullOrWhiteSpace(values.ProviderUserName) ||
            values.ProviderUserName.Trim().Length > 300 ||
            string.IsNullOrWhiteSpace(values.SenderName) ||
            values.SenderName.Trim().Length > 300 ||
            values.MessageCharactersLength is < 1 or > 1000 ||
            values.Priority is < 1 or > 1000 ||
            string.IsNullOrWhiteSpace(values.SuccessResponsePrefix) ||
            values.SuccessResponsePrefix.Trim().Length > 200 ||
            (requirePassword &&
             (string.IsNullOrWhiteSpace(values.ProviderPassword) ||
              values.ProviderPassword.Length > 300)))
        {
            throw new ValidationException("SMS provider values are invalid.");
        }

        var template = values.ApiUrlTemplate?.Trim();
        if (string.IsNullOrWhiteSpace(template) || template.Length > 2000)
        {
            throw new ValidationException("SMS provider URL template is invalid.");
        }

        string url;
        try
        {
            url = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                template, "sample-user", "sample-password", "sample-sender",
                "00967777661929", "sample-message");
        }
        catch (FormatException)
        {
            throw new ValidationException("SMS provider URL template is invalid.");
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed) ||
            parsed.Scheme != Uri.UriSchemeHttps || parsed.UserInfo.Length > 0 ||
            new[] { "sample-user", "sample-password", "sample-sender",
                "00967777661929", "sample-message" }
                .Any(part => !url.Contains(part, StringComparison.Ordinal)))
        {
            throw new ValidationException(
                "SMS provider URL must be HTTPS and include placeholders {0} through {4}.");
        }
    }

    private void AddAudit(
        Guid actorAccountId,
        Guid providerId,
        string action,
        string? correlationId,
        DateTimeOffset now)
    {
        if (actorAccountId == Guid.Empty)
        {
            throw new ValidationException("A Platform operator is required.");
        }

        db.AuditEntries.Add(new PlatformAuditEntry
        {
            Id = Guid.NewGuid(),
            AccountId = IdentityAccountId.From(actorAccountId),
            Action = action,
            ResourceType = "platform-sms-provider",
            ResourceId = providerId.ToString("D"),
            OccurredAtUtc = now,
            CorrelationId = correlationId
        });
    }

    private static PlatformSmsProviderReadModel ToReadModel(PlatformSmsProvider provider) =>
        new(provider.Id, provider.ProviderUserName, provider.SenderName,
            provider.ApiUrlTemplate, provider.MessageCharactersLength,
            provider.Priority, provider.IsActive, provider.CreatedAtUtc,
            provider.UpdatedAtUtc, Convert.ToBase64String(provider.RowVersion),
            provider.SuccessResponsePrefix, HasPassword: true);
}
