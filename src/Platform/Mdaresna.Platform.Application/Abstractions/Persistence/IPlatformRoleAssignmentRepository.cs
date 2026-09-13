using Mdaresna.Platform.Domain.Access;

namespace Mdaresna.Platform.Application.Abstractions.Persistence;

public interface IPlatformRoleAssignmentRepository
{
    Task<bool> HasActiveAssignmentAsync(
        IdentityAccountId accountId,
        PlatformRoleId roleId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PlatformRoleAssignment>> ListActiveForAccountAsync(
        IdentityAccountId accountId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        PlatformRoleAssignment assignment,
        CancellationToken cancellationToken = default);
}
