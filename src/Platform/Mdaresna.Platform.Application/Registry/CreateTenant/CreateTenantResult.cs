using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Registry.CreateTenant;

public sealed record CreateTenantResult(
    TenantId TenantId,
    string DisplayName,
    TenantStatus Status,
    bool WasCreated);
