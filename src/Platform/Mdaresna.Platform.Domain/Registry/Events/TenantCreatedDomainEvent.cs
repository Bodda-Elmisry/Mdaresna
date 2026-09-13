using Mdaresna.SharedKernel.Domain;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Domain.Registry.Events;

public sealed record TenantCreatedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    TenantId TenantId,
    string DisplayName) : IDomainEvent;
