using Mdaresna.Platform.Domain.Access;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Registry.Lifecycle;

public sealed record TransitionSchoolCommand(
    TenantId TenantId,
    SchoolId SchoolId,
    SchoolLifecycleAction Action,
    IdentityAccountId RequestedByAccountId,
    long ExpectedVersion,
    Guid CorrelationId,
    string? Reason = null,
    Guid? CausationId = null,
    string? TraceParent = null);
