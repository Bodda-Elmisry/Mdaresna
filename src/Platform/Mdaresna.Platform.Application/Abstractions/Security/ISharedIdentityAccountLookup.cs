using Mdaresna.Platform.Domain.Access;

namespace Mdaresna.Platform.Application.Abstractions.Security;

public interface ISharedIdentityAccountLookup
{
    Task<bool> ExistsAsync(
        IdentityAccountId accountId,
        CancellationToken cancellationToken = default);
}
