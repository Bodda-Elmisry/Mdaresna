using System.Text.Json;
using System.Text.Json.Serialization;
using Mdaresna.IntegrationContracts.Messaging;

namespace Mdaresna.IntegrationContracts.Serialization;

public static class IntegrationJsonSerializer
{
    public static JsonSerializerOptions CreateOptions() => new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        NumberHandling = JsonNumberHandling.Strict,
        WriteIndented = false
    };

    public static string Serialize<TMessage>(IntegrationMessageEnvelope<TMessage> message)
        where TMessage : class, IIntegrationMessage
    {
        ArgumentNullException.ThrowIfNull(message);
        return JsonSerializer.Serialize(message, CreateOptions());
    }

    public static IntegrationMessageEnvelope<TMessage> Deserialize<TMessage>(string json)
        where TMessage : class, IIntegrationMessage
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return JsonSerializer.Deserialize<IntegrationMessageEnvelope<TMessage>>(
                   json,
                   CreateOptions())
               ?? throw new JsonException("The integration message payload was null.");
    }
}
