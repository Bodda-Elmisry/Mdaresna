using Mdaresna.Platform.Application.Access.Staff;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.IdentityAuth.Staff;

/// <summary>Pages Platform-owned staff, then enriches only that page from the shared Identity database.</summary>
public sealed class PlatformStaffDirectory(
    PlatformDbContext platformDb,
    IdentityDbContext identityDb) : IPlatformStaffDirectory
{
    public async Task<PlatformStaffSummary> SummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var staff = await platformDb.LocalUsers.AsNoTracking()
            .Select(user => new { user.PersonId, user.Status })
            .ToArrayAsync(cancellationToken);
        if (staff.Length == 0) return new PlatformStaffSummary(0, 0, 0, 0, []);

        var personIds = staff.Select(user => user.PersonId).ToArray();
        var identityStatuses = await identityDb.Accounts.AsNoTracking()
            .Where(account => personIds.Contains(account.Id))
            .Select(account => new { account.Id, account.Status })
            .ToDictionaryAsync(account => account.Id, account => account.Status, cancellationToken);
        var localStatuses = staff.ToDictionary(user => user.PersonId, user => user.Status);
        var typedPersonIds = personIds.Select(IdentityAccountId.From).ToArray();
        var assignments = await (
            from assignment in platformDb.RoleAssignments.AsNoTracking()
            join role in platformDb.Roles.AsNoTracking() on assignment.RoleId equals role.Id
            where assignment.RevokedAtUtc == null && typedPersonIds.Contains(assignment.AccountId)
            select new
            {
                AccountId = assignment.AccountId.Value,
                RoleId = role.Id.Value,
                role.Key,
                role.DisplayName,
                role.IsActive
            }).ToArrayAsync(cancellationToken);

        bool IsActive(Guid accountId, bool hasActiveRole) =>
            localStatuses.TryGetValue(accountId, out var localStatus) && localStatus == "Active" &&
            identityStatuses.TryGetValue(accountId, out var identityStatus) &&
            identityStatus == AccountStatus.Active && hasActiveRole;

        var activeStaff = staff.Count(user => IsActive(user.PersonId,
            assignments.Any(assignment => assignment.AccountId == user.PersonId && assignment.IsActive)));
        var roleSummaries = assignments
            .GroupBy(assignment => new
            {
                assignment.RoleId,
                assignment.Key,
                assignment.DisplayName,
                assignment.IsActive
            })
            .Select(group =>
            {
                var members = group.GroupBy(assignment => assignment.AccountId)
                    .Select(member => member.Key).ToArray();
                var activeCount = members.Count(accountId => IsActive(accountId, group.Key.IsActive));
                return new PlatformStaffRoleSummary(group.Key.RoleId, group.Key.Key,
                    group.Key.DisplayName, activeCount, members.Length - activeCount);
            })
            .OrderBy(role => role.DisplayName)
            .ThenBy(role => role.Key)
            .ToArray();

        return new PlatformStaffSummary(staff.Length, activeStaff, staff.Length - activeStaff,
            roleSummaries.Sum(role => role.ActiveCount + role.InactiveCount), roleSummaries);
    }

    public async Task<PlatformStaffDirectoryPage> ListAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default,
        string? search = null, Guid? roleId = null)
    {
        if (pageNumber < 1 || pageSize is < 1 or > 100 || (long)(pageNumber - 1) * pageSize > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        var term = search?.Trim();
        if (term?.Length > 100) throw new ArgumentException("Search is too long.", nameof(search));
        if (roleId == Guid.Empty) throw new ArgumentException("Role ID cannot be empty.", nameof(roleId));

        var staff = platformDb.LocalUsers.AsNoTracking().AsQueryable();
        if (roleId is { } selectedRoleId)
        {
            var selectedRole = PlatformRoleId.From(selectedRoleId);
            var matchingRoleIds = await platformDb.RoleAssignments.AsNoTracking()
                .Where(assignment => assignment.RoleId == selectedRole && assignment.RevokedAtUtc == null)
                .Select(assignment => assignment.AccountId).Distinct().ToArrayAsync(cancellationToken);
            var rawMatchingRoleIds = matchingRoleIds.Select(id => id.Value).ToArray();
            staff = staff.Where(user => rawMatchingRoleIds.Contains(user.PersonId));
        }
        if (!string.IsNullOrWhiteSpace(term))
        {
            var normalized = term.ToLowerInvariant();
            var matchingIdentifiers = await identityDb.LoginIdentifiers.AsNoTracking()
                .Where(identifier => identifier.SchoolId == null &&
                    (identifier.Type == LoginIdentifierType.Phone || identifier.Type == LoginIdentifierType.Email) &&
                    identifier.NormalizedValue.ToLower().Contains(normalized))
                .Select(identifier => identifier.AccountId).Distinct().ToArrayAsync(cancellationToken);
            var matchingNames = await identityDb.Accounts.AsNoTracking()
                .Where(account => account.DisplayName != null && account.DisplayName.ToLower().Contains(normalized))
                .Select(account => account.Id).ToArrayAsync(cancellationToken);
            var matchingRoles = await (
                from assignment in platformDb.RoleAssignments.AsNoTracking()
                join role in platformDb.Roles.AsNoTracking() on assignment.RoleId equals role.Id
                where assignment.RevokedAtUtc == null &&
                      (role.Key.ToLower().Contains(normalized) || role.DisplayName.ToLower().Contains(normalized))
                select assignment.AccountId).Distinct().ToArrayAsync(cancellationToken);
            var ids = matchingIdentifiers.Concat(matchingNames).Concat(matchingRoles.Select(id => id.Value))
                .Distinct().ToArray();
            staff = staff.Where(user =>
                (user.DisplayName != null && user.DisplayName.ToLower().Contains(normalized)) ||
                user.NormalizedUserName.ToLower().Contains(normalized) ||
                ids.Contains(user.PersonId));
        }

        var total = await staff.CountAsync(cancellationToken);
        var page = await staff.OrderBy(user => user.DisplayName).ThenBy(user => user.Id)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(user => new { user.PersonId, user.UserName, user.DisplayName, user.Status })
            .ToArrayAsync(cancellationToken);
        if (page.Length == 0) return new PlatformStaffDirectoryPage([], total, pageNumber, pageSize);

        var idsOnPage = page.Select(user => user.PersonId).ToArray();
        var people = await identityDb.Accounts.AsNoTracking()
            .Where(account => idsOnPage.Contains(account.Id))
            .Select(account => new { account.Id, account.DisplayName, account.Status })
            .ToDictionaryAsync(account => account.Id, cancellationToken);
        var contacts = await identityDb.LoginIdentifiers.AsNoTracking()
            .Where(identifier => idsOnPage.Contains(identifier.AccountId) && identifier.SchoolId == null &&
                identifier.IsPrimary && (identifier.Type == LoginIdentifierType.Phone ||
                                         identifier.Type == LoginIdentifierType.Email))
            .Select(identifier => new { identifier.AccountId, identifier.Type, identifier.DisplayValue })
            .ToArrayAsync(cancellationToken);
        var images = await identityDb.AccountProfileImages.AsNoTracking()
            .Where(image => idsOnPage.Contains(image.AccountId))
            .Select(image => image.AccountId).ToArrayAsync(cancellationToken);
        var imageIds = images.ToHashSet();
        var typedPageIds = idsOnPage.Select(IdentityAccountId.From).ToArray();
        var roleRows = await (
            from assignment in platformDb.RoleAssignments.AsNoTracking()
            join role in platformDb.Roles.AsNoTracking() on assignment.RoleId equals role.Id
            where assignment.RevokedAtUtc == null && typedPageIds.Contains(assignment.AccountId)
            select new { assignment.AccountId, AssignmentId = assignment.Id,
                RoleId = role.Id, role.Key, role.DisplayName, role.IsActive })
            .ToArrayAsync(cancellationToken);
        var rolesByPerson = roleRows.GroupBy(row => row.AccountId.Value).ToDictionary(group => group.Key,
            group => (IReadOnlyList<PlatformStaffRole>)group.OrderBy(row => row.Key)
                .Select(row => new PlatformStaffRole(row.RoleId.Value, row.Key, row.DisplayName,
                    row.AssignmentId.Value, row.IsActive)).ToArray());

        var items = page.Select(user =>
        {
            people.TryGetValue(user.PersonId, out var person);
            rolesByPerson.TryGetValue(user.PersonId, out var roles);
            roles ??= [];
            var phone = contacts.FirstOrDefault(contact => contact.AccountId == user.PersonId &&
                contact.Type == LoginIdentifierType.Phone)?.DisplayValue;
            var email = contacts.FirstOrDefault(contact => contact.AccountId == user.PersonId &&
                contact.Type == LoginIdentifierType.Email)?.DisplayValue;
            var status = user.Status == "PendingActivation" ? "Requested" : user.Status;
            return new PlatformStaffDirectoryItem(user.PersonId, user.DisplayName ?? person?.DisplayName,
                email, person?.Status.ToString() ?? "MissingIdentity",
                status == "Active" && person?.Status == AccountStatus.Active && roles.Any(role => role.IsRoleActive),
                roles, phone, user.UserName, status, imageIds.Contains(user.PersonId));
        }).ToArray();
        return new PlatformStaffDirectoryPage(items, total, pageNumber, pageSize);
    }
}
