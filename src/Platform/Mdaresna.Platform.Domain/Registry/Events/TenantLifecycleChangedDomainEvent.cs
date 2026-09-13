using Mdaresna.SharedKernel.Domain;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Domain.Registry.Events;

public sealed record TenantLifecycleChangedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    TenantId TenantId,
    TenantStatus PreviousStatus,
    TenantStatus CurrentStatus,
    string? Reason) : IDomainEvent;
