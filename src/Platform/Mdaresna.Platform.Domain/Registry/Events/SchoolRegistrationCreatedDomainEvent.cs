using Mdaresna.SharedKernel.Domain;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Domain.Registry.Events;

public sealed record SchoolRegistrationCreatedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid RegistrationRequestId,
    TenantId TenantId,
    SchoolId SchoolId,
    SchoolCode SchoolCode) : IDomainEvent;
