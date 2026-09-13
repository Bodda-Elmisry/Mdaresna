using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Domain.Access;

namespace Mdaresna.Platform.Api.Auth;

public sealed class PlatformPermissionAttribute : AuthorizeAttribute
{
    public PlatformPermissionAttribute(string permissionCode) =>
        Policy = PlatformPermissionPolicyProvider.PolicyPrefix +
            PermissionCode.Create(permissionCode).Value;
}

internal sealed record PlatformPermissionRequirement(PermissionCode Permission) :
    IAuthorizationRequirement;

internal sealed class PlatformPermissionHandler(IPlatformPermissionEvaluator evaluator) :
    AuthorizationHandler<PlatformPermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PlatformPermissionRequirement requirement)
    {
        if (!Guid.TryParse(
                context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,
                out var rawAccountId) ||
            rawAccountId == Guid.Empty ||
            context.User.FindFirst(PlatformTokenClaims.Purpose)?.Value !=
                PlatformTokenClaims.TokenPurpose)
        {
            return;
        }

        if (await evaluator.HasPermissionAsync(
                IdentityAccountId.From(rawAccountId),
                requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}

internal sealed class PlatformPermissionPolicyProvider(
    IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
{
    internal const string PolicyPrefix = "PlatformPermission:";
    private readonly DefaultAuthorizationPolicyProvider _fallback = new(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        _fallback.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(PolicyPrefix, StringComparison.Ordinal))
        {
            return _fallback.GetPolicyAsync(policyName);
        }

        var rawCode = policyName[PolicyPrefix.Length..];
        PermissionCode? permission = null;
        try
        {
            permission = PermissionCode.Create(rawCode);
        }
        catch (ArgumentException)
        {
            // A malformed policy must deny, not silently fall back to authentication only.
        }

        var builder = new AuthorizationPolicyBuilder(
                JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser();
        if (permission is { } valid)
        {
            builder.AddRequirements(new PlatformPermissionRequirement(valid));
        }
        else
        {
            builder.RequireAssertion(_ => false);
        }

        return Task.FromResult<AuthorizationPolicy?>(builder.Build());
    }
}
