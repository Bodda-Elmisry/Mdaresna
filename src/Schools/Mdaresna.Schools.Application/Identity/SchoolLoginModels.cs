namespace Mdaresna.Schools.Application.Identity;

public sealed record SchoolLoginIdentifier(string UserName, string SchoolCode)
{
    public static SchoolLoginIdentifier Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var parts = value.Trim().Split('@', StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || parts.Any(string.IsNullOrWhiteSpace) || parts[0].Length > 100 || parts[1].Length > 32)
            throw new ArgumentException("Login must use username@schoolCode format.", nameof(value));
        return new SchoolLoginIdentifier(parts[0], parts[1].ToUpperInvariant());
    }
}

public sealed record SchoolLoginTarget(
    Guid TenantId, Guid SchoolId, string SchoolCode, string Provider, string ConnectionString);

public interface ISchoolLoginTenantResolver
{
    Task<SchoolLoginTarget?> ResolveAsync(string schoolCode, CancellationToken cancellationToken = default);
}

public sealed record SchoolLoginResult(
    Guid UserId, Guid PersonId, Guid TenantId, Guid SchoolId, string SchoolCode,
    string UserName, string DisplayName, string SecurityStamp, long PermissionsVersion,
    IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions);

public interface ISchoolLoginService
{
    Task<SchoolLoginResult?> LoginAsync(string login, string password, CancellationToken cancellationToken = default);
}
