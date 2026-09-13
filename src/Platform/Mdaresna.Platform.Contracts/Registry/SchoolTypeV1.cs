using System.Text.Json.Serialization;

namespace Mdaresna.Platform.Contracts.Registry;

[JsonConverter(typeof(JsonStringEnumConverter<SchoolTypeV1>))]
public enum SchoolTypeV1
{
    Private = 1,
    Government = 2
}
