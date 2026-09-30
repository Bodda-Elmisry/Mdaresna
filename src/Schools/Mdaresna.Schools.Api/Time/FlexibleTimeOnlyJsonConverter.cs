using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mdaresna.Schools.Api.Time;

public sealed class FlexibleTimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Time must be a string.");

        var value = reader.GetString();
        if (!string.IsNullOrWhiteSpace(value) &&
            TimeOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
            return time;

        throw new JsonException("Time must use HH:mm or HH:mm:ss format.");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
}
