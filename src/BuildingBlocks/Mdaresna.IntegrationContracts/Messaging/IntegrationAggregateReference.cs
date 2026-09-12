using System.Text.Json.Serialization;

namespace Mdaresna.IntegrationContracts.Messaging;

public sealed record IntegrationAggregateReference
{
    [JsonConstructor]
    public IntegrationAggregateReference(string type, Guid id, long? version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);

        if (id == Guid.Empty)
        {
            throw new ArgumentException("Aggregate id cannot be empty.", nameof(id));
        }

        if (version is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(version),
                "Aggregate version must be greater than zero when supplied.");
        }

        Type = type;
        Id = id;
        Version = version;
    }

    [JsonPropertyName("type")]
    public string Type { get; }

    [JsonPropertyName("id")]
    public Guid Id { get; }

    [JsonPropertyName("version")]
    public long? Version { get; }
}
