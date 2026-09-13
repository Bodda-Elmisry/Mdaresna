using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Registry.Read;

public sealed record ListTenantsQuery(
    string? Search = null,
    TenantStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 20);

public sealed record ListSchoolsQuery(
    TenantId? TenantId = null,
    string? Search = null,
    SchoolLifecycleStatus? Status = null,
    SchoolType? SchoolType = null,
    int PageNumber = 1,
    int PageSize = 20);
