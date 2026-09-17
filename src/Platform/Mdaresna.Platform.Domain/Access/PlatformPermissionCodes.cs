namespace Mdaresna.Platform.Domain.Access;

public static class PlatformPermissionCodes
{
    public static readonly PermissionCode SchoolsRead = PermissionCode.Create("platform.schools.read");
    public static readonly PermissionCode SchoolsManage = PermissionCode.Create("platform.schools.manage");
    public static readonly PermissionCode SchoolsActivate = PermissionCode.Create("platform.schools.activate");
    public static readonly PermissionCode SchoolMigrationsExecute = PermissionCode.Create("platform.schools.migrations.execute");
    public static readonly PermissionCode AccessManage = PermissionCode.Create("platform.access.manage");
    public static readonly PermissionCode BillingRead = PermissionCode.Create("platform.billing.read");
    public static readonly PermissionCode BillingManage = PermissionCode.Create("platform.billing.manage");
    public static readonly PermissionCode PaymentsApprove = PermissionCode.Create("platform.payments.approve");
    public static readonly PermissionCode SupportManage = PermissionCode.Create("platform.support.manage");
    public static readonly PermissionCode DeploymentsManage = PermissionCode.Create("platform.deployments.manage");
    public static readonly PermissionCode AuditRead = PermissionCode.Create("platform.audit.read");

    public static IReadOnlySet<PermissionCode> All { get; } = new HashSet<PermissionCode>
    {
        SchoolsRead,
        SchoolsManage,
        SchoolsActivate,
        SchoolMigrationsExecute,
        AccessManage,
        BillingRead,
        BillingManage,
        PaymentsApprove,
        SupportManage,
        DeploymentsManage,
        AuditRead
    };
}
