using Mdaresna.Platform.Domain.Access;

namespace Mdaresna.Platform.Application.Abstractions.Security;

public interface ISharedIdentityAccountProvisioner
{
    Task<IdentityAccountId> GetOrCreateByPhoneAsync(
        Guid registrationRequestId,
        string ownerName,
        string ownerPhone,
        CancellationToken cancellationToken = default);
}
