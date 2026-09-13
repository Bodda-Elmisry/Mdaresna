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
    long Version);
