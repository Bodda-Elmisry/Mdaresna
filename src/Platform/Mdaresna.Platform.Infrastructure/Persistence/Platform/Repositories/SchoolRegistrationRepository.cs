using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Repositories;

internal sealed class SchoolRegistrationRepository(PlatformDbContext dbContext) :
    ISchoolRegistrationRepository
{
    public Task<SchoolRegistration?> FindByIdAsync(
        SchoolId schoolId,
        CancellationToken cancellationToken = default) =>
        dbContext.Schools.SingleOrDefaultAsync(x => x.Id == schoolId, cancellationToken);

    public Task<SchoolRegistration?> FindByRegistrationRequestIdAsync(
        Guid registrationRequestId,
        CancellationToken cancellationToken = default) =>
        dbContext.Schools.SingleOrDefaultAsync(
            x => x.RegistrationRequestId == registrationRequestId,
            cancellationToken);

    public Task<bool> IsSchoolCodeInUseAsync(
        SchoolCode schoolCode,
        CancellationToken cancellationToken = default) =>
        dbContext.Schools.AnyAsync(x => x.Code == schoolCode, cancellationToken);

    public async Task AddAsync(
        SchoolRegistration registration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(registration);
        await dbContext.Schools.AddAsync(registration, cancellationToken);
    }
}
