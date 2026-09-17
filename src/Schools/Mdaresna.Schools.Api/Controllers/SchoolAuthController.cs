using System.ComponentModel.DataAnnotations;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Application.Identity;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController]
[Route("api/schools/v1/auth")]
public sealed class SchoolAuthController(
    ISchoolLoginService loginService,
    ISchoolAccessTokenIssuer tokenIssuer,
    ISchoolDbContextFactory dbFactory,
    SchoolOwnerActivationService ownerActivation) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("owner-activation/start")]
    public async Task<IActionResult> StartOwnerActivation(
        [FromBody] StartSchoolOwnerActivationRequest request, CancellationToken cancellationToken)
    {
        var result = await ownerActivation.StartAsync(request.Login, cancellationToken);
        return result == SchoolOwnerActivationStartResult.DeliveryFailed
            ? StatusCode(StatusCodes.Status503ServiceUnavailable,
                ApiResponse<object?>.Failure(503, "auth.activation_delivery_failed",
                    "The activation code could not be delivered. Try again.",
                    correlationId: HttpContext.TraceIdentifier))
            : Accepted(ApiResponse<object?>.Success(null, statusCode: 202,
                message: "If the account is eligible, an activation code will be sent.",
                correlationId: HttpContext.TraceIdentifier));
    }

    [AllowAnonymous]
    [HttpPost("owner-activation/complete")]
    public async Task<IActionResult> CompleteOwnerActivation(
        [FromBody] CompleteSchoolOwnerActivationRequest request, CancellationToken cancellationToken)
    {
        var completed = await ownerActivation.CompleteAsync(
            request.Login, request.Code, request.Password, cancellationToken);
        return completed
            ? NoContent()
            : BadRequest(ApiResponse<object?>.Failure(400, "auth.activation_invalid",
                "Activation data is invalid or expired.", correlationId: HttpContext.TraceIdentifier));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] SchoolLoginRequest request, CancellationToken cancellationToken)
    {
        var login = await loginService.LoginAsync(request.Login, request.Password, cancellationToken);
        if (login is null)
            return Unauthorized(ApiResponse<object?>.Failure(401, "auth.invalid_credentials",
                "The login identifier or password is invalid.", correlationId: HttpContext.TraceIdentifier));
        var token = tokenIssuer.Issue(login);
        var refresh = await CreateRefreshToken(login.SchoolCode, login.UserId, cancellationToken);
        return Ok(ApiResponse<SchoolLoginResponse>.Success(new SchoolLoginResponse(
            token.Token, refresh, token.ExpiresInSeconds, token.ExpiresAtUtc, login.UserId, login.PersonId,
            login.TenantId, login.SchoolId, login.SchoolCode, login.UserName, login.DisplayName,
            login.Roles, login.Permissions, login.PermissionsVersion), correlationId: HttpContext.TraceIdentifier));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] SchoolRefreshRequest request, CancellationToken cancellationToken)
    {
        var target = await dbFactory.ResolveAsync(request.SchoolCode.Trim().ToUpperInvariant(), cancellationToken);
        if (target is null) return Unauthorized(InvalidSession());
        await using var db = await dbFactory.CreateAsync(target.SchoolCode, cancellationToken);
        if (db is null) return Unauthorized(InvalidSession());
        var hash = HashToken(request.RefreshToken); var now = DateTimeOffset.UtcNow;
        var session = await db.LocalUserSessions.Include(x => x.User).ThenInclude(x => x.Person)
            .Include(x => x.User).ThenInclude(x => x.Credential)
            .Include(x => x.User).ThenInclude(x => x.Roles).ThenInclude(x => x.Role)
                .ThenInclude(x => x.Permissions).ThenInclude(x => x.Permission)
            .SingleOrDefaultAsync(x => x.RefreshTokenHash == hash, cancellationToken);
        if (session is null || session.RevokedAtUtc is not null || session.ExpiresAtUtc <= now || session.User.Status != LocalUserStatus.Active)
            return Unauthorized(InvalidSession());
        session.RevokedAtUtc = now; session.RevocationReason = "Rotated";
        var user = session.User;
        var roles = user.Roles.Where(x => x.Role.IsActive).Select(x => x.Role.Code).Distinct().Order().ToArray();
        var permissions = user.Roles.Where(x => x.Role.IsActive).SelectMany(x => x.Role.Permissions)
            .Where(x => x.Permission.IsActive).Select(x => x.Permission.Code).Distinct().Order().ToArray();
        var login = new SchoolLoginResult(user.Id, user.PersonId, target.TenantId, target.SchoolId, target.SchoolCode,
            user.UserName, user.Person.DisplayName, user.Credential?.SecurityStamp ?? string.Empty, user.PermissionsVersion, roles, permissions);
        var access = tokenIssuer.Issue(login); var refresh = NewToken();
        db.LocalUserSessions.Add(NewSession(user.Id, refresh, now)); await db.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<SchoolTokenRefreshResponse>.Success(new SchoolTokenRefreshResponse(
            access.Token, refresh, access.ExpiresInSeconds, access.ExpiresAtUtc, roles, permissions, user.PermissionsVersion),
            correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] SchoolLogoutRequest request, CancellationToken cancellationToken)
    {
        var schoolCode = User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty;
        await using var db = await dbFactory.CreateAsync(schoolCode, cancellationToken); if (db is null) return Unauthorized();
        var hash = HashToken(request.RefreshToken); var session = await db.LocalUserSessions.SingleOrDefaultAsync(x => x.RefreshTokenHash == hash, cancellationToken);
        if (session is not null && session.RevokedAtUtc is null)
        {
            session.RevokedAtUtc = DateTimeOffset.UtcNow; session.RevocationReason = "Logout"; await db.SaveChangesAsync(cancellationToken);
        }
        return NoContent();
    }

    private async Task<string> CreateRefreshToken(string schoolCode, Guid userId, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateAsync(schoolCode, ct)
            ?? throw new InvalidOperationException("School database became unavailable after login.");
        var token = NewToken(); db.LocalUserSessions.Add(NewSession(userId, token, DateTimeOffset.UtcNow)); await db.SaveChangesAsync(ct); return token;
    }
    private static LocalUserSession NewSession(Guid userId, string token, DateTimeOffset now) => new()
    { Id = Guid.NewGuid(), UserId = userId, RefreshTokenHash = HashToken(token), CreatedAtUtc = now, ExpiresAtUtc = now.AddDays(30) };
    private static string NewToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
    private ApiResponse<object?> InvalidSession() => ApiResponse<object?>.Failure(401, "auth.invalid_session", "The session is invalid or expired.", correlationId: HttpContext.TraceIdentifier);
}

public sealed record SchoolLoginRequest([Required, MaxLength(140)] string Login, [Required, MaxLength(200)] string Password);
public sealed record StartSchoolOwnerActivationRequest([Required, MaxLength(140)] string Login);
public sealed record CompleteSchoolOwnerActivationRequest(
    [Required, MaxLength(140)] string Login,
    [Required, StringLength(8, MinimumLength = 8)] string Code,
    [Required, MinLength(12), MaxLength(1024)] string Password);
public sealed record SchoolLoginResponse(
    string AccessToken, string RefreshToken, int ExpiresInSeconds, DateTimeOffset ExpiresAtUtc,
    Guid UserId, Guid PersonId, Guid TenantId, Guid SchoolId, string SchoolCode,
    string UserName, string DisplayName, IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions, long PermissionsVersion);
public sealed record SchoolRefreshRequest([Required, MaxLength(32)] string SchoolCode, [Required] string RefreshToken);
public sealed record SchoolLogoutRequest([Required] string RefreshToken);
public sealed record SchoolTokenRefreshResponse(string AccessToken, string RefreshToken, int ExpiresInSeconds,
    DateTimeOffset ExpiresAtUtc, IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions, long PermissionsVersion);
