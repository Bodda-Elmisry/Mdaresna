using System.Text.Json;
using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.IdentityAuth;

public sealed class SharedIdentityAccountProvisioner(IdentityDbContext db, IClock clock)
    : ISharedIdentityAccountProvisioner
{
    public async Task<IdentityAccountId> GetOrCreateByPhoneAsync(
        Guid registrationRequestId, string ownerName, string ownerPhone,
        CancellationToken cancellationToken = default)
    {
        if (registrationRequestId == Guid.Empty) throw new ArgumentException("Registration request id is required.");
        var phone = NormalizePhone(ownerPhone);
        var name = ownerName?.Trim() ?? string.Empty;
        if (name.Length is 0 or > 200) throw new ArgumentException("Owner name must be 1-200 characters.");

        var existing = await FindAsync(phone, cancellationToken);
        if (existing.HasValue) return IdentityAccountId.From(existing.Value);

        var now = clock.UtcNow;
        var accountId = Guid.NewGuid();
        db.Accounts.Add(new Account
        {
            Id = accountId, Status = AccountStatus.PendingVerification, DisplayName = name,
            CreatedAtUtc = now, UpdatedAtUtc = now
        });
        db.LoginIdentifiers.Add(new LoginIdentifier
        {
            Id = Guid.NewGuid(), AccountId = accountId, Type = LoginIdentifierType.Phone,
            NormalizedValue = phone, DisplayValue = phone, SchoolId = null,
            IsVerified = false, IsPrimary = true, CreatedAtUtc = now
        });
        db.SecurityEvents.Add(new IdentitySecurityEvent
        {
            Id = Guid.NewGuid(), AccountId = accountId,
            EventType = "schools.registration.owner-created", Succeeded = true,
            OccurredAtUtc = now,
            MetadataJson = JsonSerializer.Serialize(new { registrationRequestId })
        });
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return IdentityAccountId.From(accountId);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            existing = await FindAsync(phone, cancellationToken);
            if (existing.HasValue) return IdentityAccountId.From(existing.Value);
            throw;
        }
    }

    private Task<Guid?> FindAsync(string phone, CancellationToken ct) => db.LoginIdentifiers
        .AsNoTracking()
        .Where(x => x.Type == LoginIdentifierType.Phone && x.SchoolId == null && x.NormalizedValue == phone)
        .Select(x => (Guid?)x.AccountId)
        .SingleOrDefaultAsync(ct);

    private static string NormalizePhone(string value)
    {
        var phone = value?.Trim() ?? string.Empty;
        return phone.Length is >= 8 and <= 16 && phone.All(char.IsAsciiDigit)
            ? phone : throw new ArgumentException("Owner phone must contain 8-16 ASCII digits.");
    }
}
