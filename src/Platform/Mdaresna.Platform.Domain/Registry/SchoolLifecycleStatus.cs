namespace Mdaresna.Platform.Domain.Registry;

public enum SchoolLifecycleStatus
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
