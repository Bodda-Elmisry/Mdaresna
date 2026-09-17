using Mdaresna.Platform.Domain.Access;

namespace Mdaresna.Platform.Application.Abstractions.Security;

public interface ISharedIdentityAccountContactReader
{
    Task<string?> GetPrimaryPhoneAsync(IdentityAccountId accountId,
        CancellationToken cancellationToken = default);
}
