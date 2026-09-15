using Mdaresna.Platform.Contracts.Registry;
using Mdaresna.Platform.Domain.Registry;

namespace Mdaresna.Platform.Application.Registry;

internal static class ContractMappings
{
    public static SchoolType ToDomain(this SchoolTypeV1 value) => value switch
    {
        SchoolTypeV1.Private => SchoolType.Private,
        SchoolTypeV1.Government => SchoolType.Government,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown school type.")
    };

    public static DeploymentMode ToDomain(this SchoolDeploymentModeV1 value) => value switch
    {
        SchoolDeploymentModeV1.SharedSaaS => DeploymentMode.SharedSaaS,
        SchoolDeploymentModeV1.DedicatedCloud => DeploymentMode.DedicatedCloud,
        SchoolDeploymentModeV1.GovernmentOnPremises => DeploymentMode.GovernmentOnPremises,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown deployment mode.")
    };

    public static SchoolTypeV1 ToContract(this SchoolType value) => value switch
    {
        SchoolType.Private => SchoolTypeV1.Private,
        SchoolType.Government => SchoolTypeV1.Government,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown school type.")
    };

    public static SchoolDeploymentModeV1 ToContract(this DeploymentMode value) => value switch
    {
        DeploymentMode.SharedSaaS => SchoolDeploymentModeV1.SharedSaaS,
        DeploymentMode.DedicatedCloud => SchoolDeploymentModeV1.DedicatedCloud,
        DeploymentMode.GovernmentOnPremises => SchoolDeploymentModeV1.GovernmentOnPremises,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown deployment mode.")
    };

    public static SchoolLifecycleStatusV1 ToContract(this SchoolLifecycleStatus value) => value switch
    {
        SchoolLifecycleStatus.Draft => SchoolLifecycleStatusV1.Draft,
        SchoolLifecycleStatus.PendingVerification => SchoolLifecycleStatusV1.PendingVerification,
        SchoolLifecycleStatus.Approved => SchoolLifecycleStatusV1.Approved,
        SchoolLifecycleStatus.Provisioning => SchoolLifecycleStatusV1.Provisioning,
        SchoolLifecycleStatus.ProvisioningFailed => SchoolLifecycleStatusV1.ProvisioningFailed,
        SchoolLifecycleStatus.Active => SchoolLifecycleStatusV1.Active,
        SchoolLifecycleStatus.Suspended => SchoolLifecycleStatusV1.Suspended,
        SchoolLifecycleStatus.Closed => SchoolLifecycleStatusV1.Closed,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown lifecycle status.")
    };
}
