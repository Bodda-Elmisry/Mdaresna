using System.ComponentModel.DataAnnotations;
using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Api.Errors;
using Mdaresna.Platform.Infrastructure.IdentityAuth;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Application.Access.Staff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Api.Controllers.Auth;

[ApiController]
[Route("api/platform/v1/auth")]
public sealed class PlatformAuthController(
    PlatformLoginService loginService,
    AccountAppLanguageService languageService,
    IPlatformAccessTokenIssuer tokenIssuer,
    PlatformDbContext platformDb,
    IPlatformStaffManagement staffManagement) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("platform-login")]
    [HttpPost("first-owner/activation/start")]
    public async Task<IResult> StartFirstOwnerActivation(
        [FromBody] StartFirstOwnerActivationRequest request,
        [FromServices] PlatformFirstOwnerActivationService activationService,
        CancellationToken cancellationToken)
    {
        await activationService.StartAsync(request.Phone, cancellationToken);
        return ApiResponseWriter.ToResult(ApiResponse<object?>.Success(
            null,
            statusCode: 202,
            message: "If this account is eligible, an activation code will be sent.",
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [AllowAnonymous]
    [EnableRateLimiting("platform-login")]
    [HttpPost("first-owner/activation/complete")]
    public async Task<IResult> CompleteFirstOwnerActivation(
        [FromBody] CompleteFirstOwnerActivationRequest request,
        [FromServices] PlatformFirstOwnerActivationService activationService,
        CancellationToken cancellationToken)
    {
        var completed = await activationService.CompleteAsync(
            request.Phone, request.Code, request.Password, cancellationToken);
        return completed
            ? ApiResponseWriter.ToResult(ApiResponse<object?>.Success(
                null,
                message: "Account activated. Sign in with your password.",
                correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)))
            : ApiResponseWriter.ToResult(ApiResponse<object?>.Failure(
                400,
                "auth.activation_invalid",
                "The activation could not be completed.",
                correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [AllowAnonymous]
    [EnableRateLimiting("platform-login")]
    [HttpPost("password-reset/start")]
    public async Task<IResult> StartPasswordReset(
        [FromBody] StartPasswordResetRequest request,
        [FromServices] PlatformPasswordResetService resetService,
        CancellationToken cancellationToken)
    {
        await resetService.StartAsync(request.Phone, cancellationToken);
        return ApiResponseWriter.ToResult(ApiResponse<object?>.Success(
            null,
            statusCode: 202,
            message: "If this account is eligible, a password reset code will be sent.",
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [AllowAnonymous]
    [EnableRateLimiting("platform-login")]
    [HttpPost("password-reset/complete")]
    public async Task<IResult> CompletePasswordReset(
        [FromBody] CompletePasswordResetRequest request,
        [FromServices] PlatformPasswordResetService resetService,
        CancellationToken cancellationToken)
    {
        var completed = await resetService.CompleteAsync(
            request.Phone, request.Code, request.NewPassword, cancellationToken);
        return completed
            ? ApiResponseWriter.ToResult(ApiResponse<object?>.Success(
                null,
                message: "Password reset. Sign in with your new password.",
                correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)))
            : ApiResponseWriter.ToResult(ApiResponse<object?>.Failure(
                400,
                "auth.password_reset_invalid",
                "The password reset could not be completed.",
                correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [AllowAnonymous]
    [EnableRateLimiting("platform-login")]
    [HttpPost("login")]
    public async Task<IResult> Login(
        [FromBody] PlatformLoginRequest request,
        CancellationToken cancellationToken)
    {
        var login = await loginService.LoginAsync(
            request.Identifier,
            request.Password,
            cancellationToken);

        if (login is null)
        {
            return ApiResponseWriter.ToResult(
                ApiResponse<object?>.Failure(
                    401,
                    "auth.invalid_credentials",
                    "The credentials are invalid or this account cannot access the Platform.",
                    correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
        }

        var access = tokenIssuer.Issue(login);
        var preferredLanguage = await languageService.GetStoredAsync(
            login.AccountId, AccountAppLanguageService.PlatformApp, cancellationToken);
        var accountId = IdentityAccountId.From(login.AccountId);
        var roles = await (
            from assignment in platformDb.RoleAssignments.AsNoTracking()
            join role in platformDb.Roles.AsNoTracking() on assignment.RoleId equals role.Id
            where assignment.AccountId == accountId && assignment.RevokedAtUtc == null && role.IsActive
            orderby role.Key
            select new PlatformLoginRole(role.Key, role.DisplayName))
            .ToArrayAsync(cancellationToken);
        var permissions = await (
            from assignment in platformDb.RoleAssignments.AsNoTracking()
            join role in platformDb.Roles.AsNoTracking() on assignment.RoleId equals role.Id
            join permission in platformDb.RolePermissions.AsNoTracking() on role.Id equals permission.RoleId
            where assignment.AccountId == accountId && assignment.RevokedAtUtc == null && role.IsActive
            select permission.PermissionCode)
            .ToArrayAsync(cancellationToken);
        var permissionCodes = permissions.Select(code => code.Value)
            .Distinct(StringComparer.Ordinal).OrderBy(code => code, StringComparer.Ordinal).ToArray();
        return ApiResponseWriter.ToResult(
            ApiResponse<PlatformLoginResponse>.Success(
                new PlatformLoginResponse(
                    access.Token,
                    "Bearer",
                    access.ExpiresInSeconds,
                    access.ExpiresAtUtc,
                    login.AccountId,
                    login.DisplayName,
                    preferredLanguage,
                    roles,
                    permissionCodes),
                correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [AllowAnonymous]
    [EnableRateLimiting("platform-login")]
    [HttpPost("staff-activation/start")]
    public async Task<IResult> StartStaffActivation(
        [FromBody] StartStaffActivationRequest request, CancellationToken cancellationToken)
    {
        await staffManagement.StartActivationAsync(request.Phone, cancellationToken);
        return ApiResponseWriter.ToResult(ApiResponse<object?>.Success(null, statusCode: 202,
            message: "If this invitation is eligible, an activation code will be sent.",
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [AllowAnonymous]
    [EnableRateLimiting("platform-login")]
    [HttpPost("staff-activation/complete")]
    public async Task<IResult> CompleteStaffActivation(
        [FromBody] CompleteStaffActivationRequest request, CancellationToken cancellationToken)
    {
        var completed = await staffManagement.CompleteActivationAsync(
            request.Phone, request.Code, request.Password, cancellationToken);
        return completed
            ? ApiResponseWriter.ToResult(ApiResponse<object?>.Success(null,
                message: "Invitation accepted. Sign in with your Platform username and password.",
                correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)))
            : ApiResponseWriter.ToResult(ApiResponse<object?>.Failure(400,
                "auth.staff_activation_invalid", "The invitation could not be completed.",
                correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }
}

public sealed record StartFirstOwnerActivationRequest(
    [Required, MaxLength(32)] string Phone);

public sealed record CompleteFirstOwnerActivationRequest(
    [Required, MaxLength(32)] string Phone,
    [Required, StringLength(8, MinimumLength = 8)] string Code,
    [Required, MinLength(12), MaxLength(1024)] string Password);

public sealed record StartPasswordResetRequest(
    [Required, MaxLength(32)] string Phone);

public sealed record CompletePasswordResetRequest(
    [Required, MaxLength(32)] string Phone,
    [Required, StringLength(8, MinimumLength = 8)] string Code,
    [Required, MinLength(12), MaxLength(1024)] string NewPassword);

public sealed record PlatformLoginRequest(
    [Required, MaxLength(320)] string Identifier,
    [Required, MaxLength(1024)] string Password);

public sealed record PlatformLoginResponse(
    string AccessToken,
    string TokenType,
    int ExpiresInSeconds,
    DateTimeOffset ExpiresAtUtc,
    Guid AccountId,
    string? DisplayName,
    string? PreferredLanguage,
    IReadOnlyList<PlatformLoginRole> Roles,
    IReadOnlyList<string> PermissionCodes);

public sealed record PlatformLoginRole(string Key, string DisplayName);

public sealed record StartStaffActivationRequest([Required, MaxLength(32)] string Phone);
public sealed record CompleteStaffActivationRequest(
    [Required, MaxLength(32)] string Phone,
    [Required, StringLength(8, MinimumLength = 8)] string Code,
    [Required, MinLength(12), MaxLength(1024)] string Password);
