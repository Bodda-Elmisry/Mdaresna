using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Abstractions.Persistence;

public interface ISchoolRegistrationRepository
{
    Task<SchoolRegistration?> FindByIdAsync(
        SchoolId schoolId,
        CancellationToken cancellationToken = default);

    Task<SchoolRegistration?> FindByRegistrationRequestIdAsync(
        Guid registrationRequestId,
        CancellationToken cancellationToken = default);

    Task<SchoolRegistration?> FindByCodeAsync(
        SchoolCode schoolCode,
        CancellationToken cancellationToken = default);

    Task<bool> IsSchoolCodeInUseAsync(
        SchoolCode schoolCode,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        SchoolRegistration registration,
        CancellationToken cancellationToken = default);
}
