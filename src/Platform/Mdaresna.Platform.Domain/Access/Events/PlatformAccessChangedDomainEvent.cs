using Mdaresna.SharedKernel.Domain;

namespace Mdaresna.Platform.Domain.Access.Events;

public sealed record PlatformAccessChangedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    string ChangeType,
    Guid SubjectId,
    Guid ChangedByAccountId) : IDomainEvent;
