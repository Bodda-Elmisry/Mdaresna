using System.Security.Cryptography;
using System.Text;
using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Application.Registry.DatabaseEndpoints;
using Mdaresna.Platform.Domain.Registry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mdaresna.Platform.Api.Auth;

namespace Mdaresna.Platform.Api.Controllers.Internal;

[ApiController]
[AllowAnonymous]
[PlatformInternalService]
[Route("api/platform/v1/internal/school-login-targets")]
public sealed class SchoolLoginTargetsController(
    ISchoolRegistrationRepository schools,
    ISchoolDatabaseEndpointResolver endpoints,
    IConfiguration configuration) : ControllerBase
{
    [HttpGet("{schoolCode}")]
    public async Task<IActionResult> Resolve(string schoolCode, CancellationToken cancellationToken)
    {
        var configuredKey = configuration["InternalServices:ApiKey"];
        var suppliedKey = Request.Headers["X-Mdaresna-Internal-Key"].ToString();
        if (string.IsNullOrWhiteSpace(configuredKey) || !FixedEquals(configuredKey, suppliedKey))
            return Unauthorized();

        SchoolCode code;
        try { code = SchoolCode.Create(schoolCode); }
        catch (ArgumentException) { return NotFound(); }

        var school = await schools.FindByCodeAsync(code, cancellationToken);
        if (school is null || school.Status != SchoolLifecycleStatus.Active) return NotFound();

        try
        {
            var endpoint = await endpoints.ResolvePrimaryAsync(school.Id, cancellationToken: cancellationToken);
            return Ok(new SchoolLoginTargetResponse(
                school.TenantId.Value, school.Id.Value, school.Code.Value, school.Status.ToString(),
                endpoint.Provider.ToString(), endpoint.Host, endpoint.Port, endpoint.DatabaseName,
                endpoint.CredentialSecretReference, endpoint.RequireTls));
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    private static bool FixedEquals(string expected, string actual)
    {
        var a = Encoding.UTF8.GetBytes(expected); var b = Encoding.UTF8.GetBytes(actual);
        return a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b);
    }
}

public sealed record SchoolLoginTargetResponse(
    Guid TenantId, Guid SchoolId, string SchoolCode, string Status, string Provider,
    string Host, int Port, string DatabaseName, string CredentialSecretReference, bool RequireTls);
