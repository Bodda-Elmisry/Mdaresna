namespace Mdaresna.Platform.Application.Access.Staff;

public sealed record PlatformStaffRole(Guid RoleId, string Key, string DisplayName);

public sealed record PlatformStaffDirectoryItem(
    Guid AccountId,
    string? DisplayName,
    string? VerifiedEmail,
    string AccountStatus,
    bool IsActive,
    IReadOnlyList<PlatformStaffRole> ActiveRoles,
    string? VerifiedPhone = null);

public sealed record PlatformStaffDirectoryPage(
    IReadOnlyList<PlatformStaffDirectoryItem> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);

/// <summary>
/// Lists identities that have ever been assigned a Platform role. A staff member is
/// active only while the central account and at least one Platform role are active.
/// </summary>
public interface IPlatformStaffDirectory
{
    Task<PlatformStaffDirectoryPage> ListAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
