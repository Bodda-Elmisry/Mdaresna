using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.IdentityAuth;

public sealed class SharedIdentityAccountContactReader(IdentityDbContext db)
    : ISharedIdentityAccountContactReader
{
    public Task<string?> GetPrimaryPhoneAsync(IdentityAccountId accountId,
        CancellationToken cancellationToken = default) => db.LoginIdentifiers.AsNoTracking()
        .Where(x => x.AccountId == (Guid)accountId && x.Type == LoginIdentifierType.Phone &&
                    x.SchoolId == null && x.IsPrimary)
        .Select(x => x.DisplayValue)
        .SingleOrDefaultAsync(cancellationToken);
}
