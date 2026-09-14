namespace Mdaresna.Platform.Application.Access.Staff;

public sealed record PlatformStaffLookupResult(bool Exists, string? DisplayName,
    string? PrimaryEmail, bool HasImage, string? StaffStatus);

public sealed record InvitePlatformStaffRequest(string Phone, string UserName,
    string? DisplayName, IReadOnlyList<Guid> RoleIds);

public sealed record UpdatePlatformStaffRequest(string UserName, string? DisplayName);

public interface IPlatformStaffManagement
{
    Task<PlatformStaffLookupResult> LookupAsync(Guid actorAccountId, string phone,
        CancellationToken cancellationToken = default);
    Task<Guid> InviteAsync(Guid actorAccountId, InvitePlatformStaffRequest request,
        CancellationToken cancellationToken = default, string? correlationId = null);
    Task StartActivationAsync(string phone, CancellationToken cancellationToken = default);
    Task<bool> CompleteActivationAsync(string phone, string code, string password,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid actorAccountId, Guid accountId, UpdatePlatformStaffRequest request,
        CancellationToken cancellationToken = default, string? correlationId = null);
    Task SetActiveAsync(Guid actorAccountId, Guid accountId, bool isActive,
        CancellationToken cancellationToken = default, string? correlationId = null);
}
