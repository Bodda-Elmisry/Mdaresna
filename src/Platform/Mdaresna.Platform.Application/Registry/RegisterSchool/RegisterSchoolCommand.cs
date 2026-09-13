using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Registry.RegisterSchool;

public sealed record RegisterSchoolCommand(
    Guid RegistrationRequestId,
    TenantId TenantId,
    string SchoolCode,
    string DisplayName,
    SchoolType SchoolType,
    DeploymentMode DeploymentMode,
    IdentityAccountId RequestedByAccountId,
    Guid CorrelationId,
    Guid? CausationId = null,
    string? TraceParent = null);
