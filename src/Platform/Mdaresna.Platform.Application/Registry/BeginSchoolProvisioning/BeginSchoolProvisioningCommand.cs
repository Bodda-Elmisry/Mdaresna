using Mdaresna.Platform.Domain.Access;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Registry.BeginSchoolProvisioning;

public sealed record BeginSchoolProvisioningCommand(
    TenantId TenantId,
    SchoolId SchoolId,
    Guid OperationId,
    IdentityAccountId RequestedByAccountId,
    Guid CorrelationId,
    Guid? CausationId = null,
    string? TraceParent = null);
