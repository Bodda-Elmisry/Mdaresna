using Mdaresna.Platform.Domain.Access;

namespace Mdaresna.Platform.Application.Abstractions.Persistence;

public interface IPlatformRoleRepository
{
    Task<PlatformRole?> FindByIdAsync(
        PlatformRoleId roleId,
        CancellationToken cancellationToken = default);

    Task<PlatformRole?> FindByKeyAsync(
        string roleKey,
        CancellationToken cancellationToken = default);

    Task AddAsync(PlatformRole role, CancellationToken cancellationToken = default);
}
