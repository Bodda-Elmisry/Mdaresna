using System.Text.Json.Serialization;

namespace Mdaresna.IntegrationContracts.Messaging;

public sealed record IntegrationMessageEnvelope<TMessage>
    where TMessage : class, IIntegrationMessage
{
    [JsonConstructor]
    public IntegrationMessageEnvelope(
        Guid messageId,
        string messageType,
        ushort schemaVersion,
        DateTimeOffset occurredAtUtc,
        string producer,
        IntegrationMessageScope scope,
        IntegrationAggregateReference? aggregate,
        Guid correlationId,
        Guid? causationId,
        string? traceParent,
        TMessage data)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("MessageId cannot be empty.", nameof(messageId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(messageType);

        if (!string.Equals(messageType, TMessage.MessageType, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"MessageType must be '{TMessage.MessageType}' for {typeof(TMessage).Name}.",
                nameof(messageType));
        }

        if (schemaVersion == 0 || schemaVersion != TMessage.SchemaVersion)
        {
            throw new ArgumentOutOfRangeException(
                nameof(schemaVersion),
                $"SchemaVersion must be {TMessage.SchemaVersion} for {typeof(TMessage).Name}.");
        }

        if (occurredAtUtc == default || occurredAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "OccurredAtUtc must be a non-default timestamp using the UTC offset.",
                nameof(occurredAtUtc));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(producer);
        ArgumentNullException.ThrowIfNull(scope);

        if (correlationId == Guid.Empty)
        {
            throw new ArgumentException("CorrelationId cannot be empty.", nameof(correlationId));
        }

        if (causationId == Guid.Empty)
        {
            throw new ArgumentException("CausationId cannot be empty when supplied.", nameof(causationId));
        }

        ArgumentNullException.ThrowIfNull(data);

        MessageId = messageId;
        MessageType = messageType;
        SchemaVersion = schemaVersion;
        OccurredAtUtc = occurredAtUtc;
        Producer = producer;
        Scope = scope;
        Aggregate = aggregate;
        CorrelationId = correlationId;
        CausationId = causationId;
        TraceParent = traceParent;
        Data = data;
    }

    [JsonPropertyName("messageId")]
    public Guid MessageId { get; }

    [JsonPropertyName("messageType")]
    public string MessageType { get; }

    [JsonPropertyName("schemaVersion")]
    public ushort SchemaVersion { get; }

    [JsonPropertyName("occurredAtUtc")]
    public DateTimeOffset OccurredAtUtc { get; }

    [JsonPropertyName("producer")]
    public string Producer { get; }

    [JsonPropertyName("scope")]
    public IntegrationMessageScope Scope { get; }

    [JsonPropertyName("aggregate")]
    public IntegrationAggregateReference? Aggregate { get; }

    [JsonPropertyName("correlationId")]
    public Guid CorrelationId { get; }

    [JsonPropertyName("causationId")]
    public Guid? CausationId { get; }

    [JsonPropertyName("traceParent")]
    public string? TraceParent { get; }

    [JsonPropertyName("data")]
    public TMessage Data { get; }

    public static IntegrationMessageEnvelope<TMessage> Create(
        DateTimeOffset occurredAtUtc,
        string producer,
        IntegrationMessageScope scope,
        TMessage data,
        IntegrationAggregateReference? aggregate = null,
        Guid? correlationId = null,
        Guid? causationId = null,
        string? traceParent = null) => new(
            Guid.NewGuid(),
            TMessage.MessageType,
            TMessage.SchemaVersion,
            occurredAtUtc,
            producer,
            scope,
            aggregate,
            correlationId ?? Guid.NewGuid(),
            causationId,
            traceParent,
            data);
}
