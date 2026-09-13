namespace Mdaresna.Platform.Application.Access.Staff;

public sealed record PlatformStaffRoleAssignmentResult(
    Guid AssignmentId,
    Guid AccountId,
    Guid RoleId,
    bool Changed);

public interface IPlatformStaffRoleManager
{
    Task<PlatformStaffRoleAssignmentResult> AssignAsync(
        Guid actorAccountId,
        Guid targetAccountId,
        Guid roleId,
        CancellationToken cancellationToken = default,
        string? correlationId = null);

    Task<PlatformStaffRoleAssignmentResult> RevokeAsync(
        Guid actorAccountId,
        Guid assignmentId,
        CancellationToken cancellationToken = default,
        string? correlationId = null);
}

public sealed class PlatformStaffAccessDeniedException : Exception
{
    public PlatformStaffAccessDeniedException()
        : base("The current Platform operator cannot manage staff access.")
    {
    }
}
