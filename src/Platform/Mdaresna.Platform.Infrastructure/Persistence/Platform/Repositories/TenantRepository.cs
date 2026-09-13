using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Repositories;

internal sealed class TenantRepository(PlatformDbContext dbContext) : ITenantRepository
{
    public Task<Tenant?> FindByIdAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default) =>
        dbContext.Tenants.SingleOrDefaultAsync(x => x.Id == tenantId, cancellationToken);

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        await dbContext.Tenants.AddAsync(tenant, cancellationToken);
    }
}
