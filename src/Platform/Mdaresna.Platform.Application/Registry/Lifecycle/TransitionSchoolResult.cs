using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Registry.Lifecycle;

public sealed record TransitionSchoolResult(
    TenantId TenantId,
    SchoolId SchoolId,
    SchoolLifecycleStatus Status,
    long Version);
