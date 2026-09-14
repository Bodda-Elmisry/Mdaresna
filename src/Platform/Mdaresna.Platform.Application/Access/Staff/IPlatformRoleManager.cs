namespace Mdaresna.Platform.Application.Access.Staff;

public sealed record PlatformRoleSaveRequest(
    string Key,
    string DisplayName,
    IReadOnlyList<string> PermissionCodes);

public interface IPlatformRoleManager
{
    Task<IReadOnlyList<string>> ListPermissionsAsync(CancellationToken cancellationToken = default);
    Task<PlatformRoleCatalogItem> CreateAsync(Guid actorAccountId, PlatformRoleSaveRequest request,
        CancellationToken cancellationToken = default, string? correlationId = null);
    Task<PlatformRoleCatalogItem> UpdateAsync(Guid actorAccountId, Guid roleId, PlatformRoleSaveRequest request,
        CancellationToken cancellationToken = default, string? correlationId = null);
    Task<PlatformRoleCatalogItem> SetActiveAsync(Guid actorAccountId, Guid roleId, bool isActive,
        CancellationToken cancellationToken = default, string? correlationId = null);
    Task DeleteAsync(Guid actorAccountId, Guid roleId,
        CancellationToken cancellationToken = default, string? correlationId = null);
}
