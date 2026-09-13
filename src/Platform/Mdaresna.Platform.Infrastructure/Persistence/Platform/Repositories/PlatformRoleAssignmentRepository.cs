using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Domain.Access;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Repositories;

internal sealed class PlatformRoleAssignmentRepository(PlatformDbContext dbContext) :
    IPlatformRoleAssignmentRepository
{
    public Task<bool> HasActiveAssignmentAsync(
        IdentityAccountId accountId,
        PlatformRoleId roleId,
        CancellationToken cancellationToken = default) =>
        dbContext.RoleAssignments.AnyAsync(
            x => x.AccountId == accountId && x.RoleId == roleId && x.RevokedAtUtc == null,
            cancellationToken);

    public async Task<IReadOnlyCollection<PlatformRoleAssignment>> ListActiveForAccountAsync(
        IdentityAccountId accountId,
        CancellationToken cancellationToken = default) =>
        await dbContext.RoleAssignments
            .Where(x => x.AccountId == accountId && x.RevokedAtUtc == null)
            .ToArrayAsync(cancellationToken);

    public async Task AddAsync(
        PlatformRoleAssignment assignment,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assignment);
        await dbContext.RoleAssignments.AddAsync(assignment, cancellationToken);
    }
}
