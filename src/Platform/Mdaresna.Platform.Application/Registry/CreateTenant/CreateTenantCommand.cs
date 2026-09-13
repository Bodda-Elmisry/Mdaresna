using Mdaresna.Platform.Domain.Access;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Registry.CreateTenant;

public sealed record CreateTenantCommand(
    TenantId TenantId,
    string DisplayName,
    string? LegalName,
    IdentityAccountId RequestedByAccountId,
    Guid CorrelationId,
    Guid? CausationId = null,
    string? TraceParent = null);
