using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Registry.RegisterSchool;

public sealed record RegisterSchoolResult(
    SchoolId SchoolId,
    TenantId TenantId,
    SchoolCode SchoolCode,
    SchoolLifecycleStatus Status,
    bool WasCreated);
