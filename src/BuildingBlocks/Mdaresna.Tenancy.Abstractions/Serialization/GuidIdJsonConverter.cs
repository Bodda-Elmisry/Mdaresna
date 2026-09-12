using System.Text.Json;
using System.Text.Json.Serialization;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Tenancy.Abstractions.Serialization;

public sealed class GuidIdJsonConverter<TId> : JsonConverter<TId>
    where TId : struct, IGuidId<TId>
{
    public override TId Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String &&
            reader.TryGetGuid(out var value) &&
            value != Guid.Empty)
        {
            return TId.From(value);
        }

        throw new JsonException(
            $"Expected a non-empty GUID string for {typeof(TId).Name}.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        TId value,
        JsonSerializerOptions options)
    {
        if (value.Value == Guid.Empty)
        {
            throw new JsonException($"{typeof(TId).Name} cannot be empty.");
        }

        writer.WriteStringValue(value.Value);
    }
}
