using System.Text.Json.Serialization;

namespace Mdaresna.Platform.Contracts.Registry;

[JsonConverter(typeof(JsonStringEnumConverter<SchoolDeploymentModeV1>))]
public enum SchoolDeploymentModeV1
{
    SharedSaaS = 1,
    DedicatedCloud = 2,
    GovernmentOnPremises = 3
}
