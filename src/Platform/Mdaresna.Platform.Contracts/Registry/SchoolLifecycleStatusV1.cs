using System.Text.Json.Serialization;

namespace Mdaresna.Platform.Contracts.Registry;

[JsonConverter(typeof(JsonStringEnumConverter<SchoolLifecycleStatusV1>))]
public enum SchoolLifecycleStatusV1
{
    Draft = 1,
    PendingVerification = 2,
    Approved = 3,
    Provisioning = 4,
    ProvisioningFailed = 5,
    Active = 6,
    Suspended = 7,
    Closed = 8
}
