using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Registry.Read;

public sealed record TenantReadModel(
    TenantId Id,
    string DisplayName,
    string? LegalName,
    TenantStatus Status,
    string? StatusReason,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    long Version);

public sealed record SchoolReadModel(
    SchoolId Id,
    TenantId TenantId,
    Guid RegistrationRequestId,
    string Code,
    string DisplayName,
    SchoolType SchoolType,
    DeploymentMode DeploymentMode,
    SchoolLifecycleStatus Status,
    Guid RequestedByAccountId,
    Guid? ProvisioningOperationId,
    string? StatusReason,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    long Version,
    string? Address = null,
    DateTimeOffset? ActivatedAtUtc = null,
    Guid? UnitTypeId = null,
    string? UnitTypeCode = null,
    string? UnitTypeName = null,
    string? Currency = null,
    string? OwnerDisplayName = null);

public sealed record SchoolTypeSummary(
    SchoolType SchoolType,
    int Count);

public sealed record SchoolStatusSummary(
    SchoolLifecycleStatus Status,
    int TotalCount,
    IReadOnlyList<SchoolTypeSummary> Types);

public sealed record SchoolDirectorySummary(
    int TotalSchools,
    IReadOnlyList<SchoolStatusSummary> Statuses);
