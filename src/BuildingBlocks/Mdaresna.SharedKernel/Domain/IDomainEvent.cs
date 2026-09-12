namespace Mdaresna.SharedKernel.Domain;

/// <summary>
/// Identifies an event raised inside one bounded context.
/// Domain events never cross a system boundary directly.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredAtUtc { get; }
}
