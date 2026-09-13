using Mdaresna.SharedKernel.Domain;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Domain.Registry.Events;

public sealed record SchoolLifecycleChangedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    TenantId TenantId,
    SchoolId SchoolId,
    SchoolLifecycleStatus PreviousStatus,
    SchoolLifecycleStatus CurrentStatus,
    Guid ChangedByAccountId,
    Guid? ProvisioningOperationId,
    string? Reason) : IDomainEvent;
