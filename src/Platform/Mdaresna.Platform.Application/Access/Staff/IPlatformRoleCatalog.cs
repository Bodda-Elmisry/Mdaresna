namespace Mdaresna.Platform.Application.Access.Staff;

public sealed record PlatformRoleCatalogItem(
    Guid RoleId,
    string Key,
    string DisplayName,
    bool IsActive,
    bool IsSystem,
    IReadOnlyList<string> PermissionCodes);

public interface IPlatformRoleCatalog
{
    Task<IReadOnlyList<PlatformRoleCatalogItem>> ListAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default);
}
