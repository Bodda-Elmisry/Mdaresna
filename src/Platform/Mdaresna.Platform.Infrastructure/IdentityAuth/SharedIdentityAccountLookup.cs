using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.IdentityAuth;

internal sealed class SharedIdentityAccountLookup(IdentityDbContext db)
    : ISharedIdentityAccountLookup
{
    public Task<bool> ExistsAsync(
        IdentityAccountId accountId,
        CancellationToken cancellationToken = default)
    {
        if (accountId.IsEmpty)
        {
            return Task.FromResult(false);
        }

        var id = (Guid)accountId;
        return db.Accounts.AsNoTracking().AnyAsync(
            account => account.Id == id,
            cancellationToken);
    }
}
