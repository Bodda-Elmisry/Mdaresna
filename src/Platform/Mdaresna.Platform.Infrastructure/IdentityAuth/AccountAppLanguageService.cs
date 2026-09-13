using Mdaresna.Platform.Infrastructure.Persistence;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.IdentityAuth;

/// <summary>
/// Central account preferences are isolated by application, not by client device.
/// Future Schools and Family APIs can use their own app code with central Identity.
/// </summary>
public sealed class AccountAppLanguageService(IdentityDbContext db)
{
    public const string PlatformApp = "platform";
    public const string SchoolsApp = "schools";
    public const string FamilyApp = "family";
    public const string DefaultLanguage = "ar";

    public static bool IsSupported(string? languageCode) => languageCode is "ar" or "en";

    public Task<string?> GetStoredAsync(
        Guid accountId, string appCode, CancellationToken cancellationToken = default)
    {
        ValidateAppCode(appCode);
        return db.AppLanguagePreferences.AsNoTracking()
            .Where(x => x.AccountId == accountId && x.AppCode == appCode)
            .Select(x => x.LanguageCode)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<string> SetAsync(
        Guid accountId, string appCode, string languageCode,
        CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty) throw new ArgumentException("Account ID is required.", nameof(accountId));
        ValidateAppCode(appCode);
        if (!IsSupported(languageCode))
            throw new ArgumentException("Language must be 'ar' or 'en'.", nameof(languageCode));

        var preference = await db.AppLanguagePreferences.SingleOrDefaultAsync(
            x => x.AccountId == accountId && x.AppCode == appCode, cancellationToken);
        if (preference is null)
        {
            db.AppLanguagePreferences.Add(new AccountAppLanguagePreference
            {
                AccountId = accountId,
                AppCode = appCode,
                LanguageCode = languageCode,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            });
        }
        else if (preference.LanguageCode != languageCode)
        {
            preference.LanguageCode = languageCode;
            preference.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }
        else
        {
            return languageCode;
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (DatabaseErrorClassifier.IsUniqueViolation(exception.InnerException))
        {
            // Two devices selected a language before either initial row existed.
            db.ChangeTracker.Clear();
            var existing = await db.AppLanguagePreferences.SingleAsync(
                x => x.AccountId == accountId && x.AppCode == appCode, cancellationToken);
            existing.LanguageCode = languageCode;
            existing.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }
        return languageCode;
    }

    private static void ValidateAppCode(string appCode)
    {
        if (appCode is not (PlatformApp or SchoolsApp or FamilyApp))
            throw new ArgumentException("Unknown application code.", nameof(appCode));
    }
}
