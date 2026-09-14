using Mdaresna.Platform.Application.Access.Staff;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.IdentityAuth.Staff;

/// <summary>
/// Read-only fan-out across the two independent databases. It does not join or
/// transact across them; account and role changes may be momentarily out of sync.
/// </summary>
public sealed class PlatformStaffDirectory(
    PlatformDbContext platformDb,
    IdentityDbContext identityDb) : IPlatformStaffDirectory
{
    public async Task<PlatformStaffDirectoryPage> ListAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                "Page number must be positive and page size must be between 1 and 100.");
        }

        var skip = (long)(pageNumber - 1) * pageSize;
        if (skip > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }

        var staffIds = platformDb.RoleAssignments
            .AsNoTracking()
            .Select(x => x.AccountId)
            .Distinct();
        var totalCount = await staffIds.CountAsync(cancellationToken);
        var pageIds = await staffIds
            .OrderBy(x => x)
            .Skip((int)skip)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
        if (pageIds.Length == 0)
        {
            return new PlatformStaffDirectoryPage([], totalCount, pageNumber, pageSize);
        }

        var rawIds = pageIds.Select(x => x.Value).ToArray();
        var accounts = await identityDb.Accounts
            .AsNoTracking()
            .Where(x => rawIds.Contains(x.Id))
            .Select(x => new { x.Id, x.DisplayName, x.Status })
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        var localDisplayNames = await platformDb.LocalUsers.AsNoTracking()
            .Where(x => rawIds.Contains(x.PersonId))
            .Select(x => new { x.PersonId, x.DisplayName })
            .ToDictionaryAsync(x => x.PersonId, x => x.DisplayName, cancellationToken);
        var emails = await identityDb.LoginIdentifiers
            .AsNoTracking()
            .Where(x => rawIds.Contains(x.AccountId) &&
                        x.Type == LoginIdentifierType.Email &&
                        x.IsVerified)
            .Select(x => new { x.AccountId, x.DisplayValue })
            .ToArrayAsync(cancellationToken);
        var emailByAccount = emails
            .GroupBy(x => x.AccountId)
            .ToDictionary(x => x.Key, x => x.First().DisplayValue);
        var phones = await identityDb.LoginIdentifiers
            .AsNoTracking()
            .Where(x => rawIds.Contains(x.AccountId) &&
                        x.Type == LoginIdentifierType.Phone &&
                        x.SchoolId == null &&
                        x.IsVerified)
            .Select(x => new { x.AccountId, x.DisplayValue })
            .ToArrayAsync(cancellationToken);
        var phoneByAccount = phones
            .GroupBy(x => x.AccountId)
            .ToDictionary(x => x.Key, x => x.First().DisplayValue);

        var activeRoleRows = await (
            from assignment in platformDb.RoleAssignments.AsNoTracking()
            join role in platformDb.Roles.AsNoTracking()
                on assignment.RoleId equals role.Id
            where pageIds.Contains(assignment.AccountId) &&
                  assignment.RevokedAtUtc == null &&
                  role.IsActive
            select new
            {
                assignment.AccountId,
                RoleId = role.Id,
                role.Key,
                role.DisplayName
            }).ToArrayAsync(cancellationToken);
        var rolesByAccount = activeRoleRows
            .GroupBy(x => x.AccountId)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<PlatformStaffRole>)x
                    .OrderBy(role => role.Key)
                    .Select(role => new PlatformStaffRole(
                        role.RoleId.Value,
                        role.Key,
                        role.DisplayName))
                    .ToArray());

        var items = pageIds.Select(id =>
        {
            var accountId = id.Value;
            accounts.TryGetValue(accountId, out var account);
            localDisplayNames.TryGetValue(accountId, out var localDisplayName);
            emailByAccount.TryGetValue(accountId, out var email);
            phoneByAccount.TryGetValue(accountId, out var phone);
            rolesByAccount.TryGetValue(id, out var roles);
            roles ??= [];

            return new PlatformStaffDirectoryItem(
                accountId,
                localDisplayName ?? account?.DisplayName,
                email,
                account?.Status.ToString() ?? "MissingIdentity",
                account?.Status == AccountStatus.Active && roles.Count > 0,
                roles,
                phone);
        }).ToArray();

        return new PlatformStaffDirectoryPage(items, totalCount, pageNumber, pageSize);
    }
}
