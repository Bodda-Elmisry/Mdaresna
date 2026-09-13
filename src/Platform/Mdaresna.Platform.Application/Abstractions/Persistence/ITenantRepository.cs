using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Abstractions.Persistence;

public interface ITenantRepository
{
    Task<Tenant?> FindByIdAsync(TenantId tenantId, CancellationToken cancellationToken = default);

    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
}
