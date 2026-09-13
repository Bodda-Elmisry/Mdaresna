using Mdaresna.Platform.Domain.Access;

namespace Mdaresna.Platform.Application.Abstractions.Security;

public interface IPlatformPermissionEvaluator
{
    Task<bool> HasPermissionAsync(
        IdentityAccountId accountId,
        PermissionCode permission,
        CancellationToken cancellationToken = default);
}
