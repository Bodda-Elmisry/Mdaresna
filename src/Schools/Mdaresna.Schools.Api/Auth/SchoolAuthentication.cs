using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Mdaresna.Schools.Application.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

namespace Mdaresna.Schools.Api.Auth;

public static class SchoolClaimTypes
{
    public const string Purpose = "token_purpose";
    public const string TenantId = "tenant_id";
    public const string SchoolId = "school_id";
    public const string SchoolCode = "school_code";
    public const string PersonId = "person_id";
    public const string UserName = "username";
    public const string Role = "role";
    public const string Permission = "permission";
    public const string PermissionsVersion = "permissions_version";
    public const string SecurityStamp = "security_stamp";
}

public static class SchoolPermissionPolicies
{
    public const string Prefix = "school-permission:";
    public const string UsersView = Prefix + "school.users.view";
    public const string UsersManage = Prefix + "school.users.manage";
    public const string RolesView = Prefix + "school.roles.view";
    public const string RolesManage = Prefix + "school.roles.manage";
    public const string FacilitiesView = Prefix + "school.facilities.view";
    public const string FacilitiesManage = Prefix + "school.facilities.manage";
    public const string FacilitiesDelete = Prefix + "school.facilities.delete";
    public const string FacilitiesRestore = Prefix + "school.facilities.restore";
    public const string AcademicsView = Prefix + "school.academics.view";
    public const string AcademicsManage = Prefix + "school.academics.manage";
    public const string AcademicsDelete = Prefix + "school.academics.delete";
    public const string AcademicsRestore = Prefix + "school.academics.restore";
    public const string OperationsView = Prefix + "school.operations.view";
    public const string OperationsManage = Prefix + "school.operations.manage";
    public const string OperationsDelete = Prefix + "school.operations.delete";
    public const string OperationsRestore = Prefix + "school.operations.restore";
    public const string CalendarView = Prefix + "school.calendar.view";
    public const string CalendarManage = Prefix + "school.calendar.manage";
    public const string CalendarDelete = Prefix + "school.calendar.delete";
    public const string CalendarRestore = Prefix + "school.calendar.restore";
}

internal sealed record SchoolPermissionRequirement(string Permission) : IAuthorizationRequirement;

internal sealed class SchoolPermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(SchoolPermissionPolicies.Prefix, StringComparison.Ordinal))
            return base.GetPolicyAsync(policyName);
        var permission = policyName[SchoolPermissionPolicies.Prefix.Length..];
        return Task.FromResult<AuthorizationPolicy?>(new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser().AddRequirements(new SchoolPermissionRequirement(permission)).Build());
    }
}

internal sealed class SchoolPermissionHandler : AuthorizationHandler<SchoolPermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SchoolPermissionRequirement requirement)
    {
        if (context.User.HasClaim(SchoolClaimTypes.Permission, requirement.Permission)) context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

internal sealed record SchoolJwtOptions(string Issuer, string Audience, byte[] SigningKey, int AccessTokenMinutes)
{
    public static SchoolJwtOptions FromConfiguration(IConfiguration configuration)
    {
        var issuer = configuration["SchoolIdentity:Jwt:Issuer"]?.Trim();
        var audience = configuration["SchoolIdentity:Jwt:Audience"]?.Trim();
        var key = Encoding.UTF8.GetBytes(configuration["SchoolIdentity:Jwt:SigningKey"] ?? string.Empty);
        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || key.Length < 32)
            throw new InvalidOperationException("SchoolIdentity JWT issuer, audience and a signing key of at least 32 bytes are required.");
        var minutes = configuration.GetValue<int?>("SchoolIdentity:Jwt:AccessTokenMinutes") ?? 15;
        if (minutes is < 1 or > 15) throw new InvalidOperationException("School access token lifetime must be between 1 and 15 minutes.");
        return new SchoolJwtOptions(issuer, audience, key, minutes);
    }
    public SymmetricSecurityKey SecurityKey() => new(SigningKey);
}

public sealed record SchoolAccessToken(string Token, int ExpiresInSeconds, DateTimeOffset ExpiresAtUtc);
public interface ISchoolAccessTokenIssuer { SchoolAccessToken Issue(SchoolLoginResult login); }

internal sealed class SchoolAccessTokenIssuer(SchoolJwtOptions options) : ISchoolAccessTokenIssuer
{
    public SchoolAccessToken Issue(SchoolLoginResult login)
    {
        var now = DateTimeOffset.UtcNow; var expires = now.AddMinutes(options.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, login.UserId.ToString("D")), new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Name, login.DisplayName), new(SchoolClaimTypes.Purpose, "school_access"),
            new(SchoolClaimTypes.TenantId, login.TenantId.ToString("D")), new(SchoolClaimTypes.SchoolId, login.SchoolId.ToString("D")),
            new(SchoolClaimTypes.SchoolCode, login.SchoolCode), new(SchoolClaimTypes.PersonId, login.PersonId.ToString("D")),
            new(SchoolClaimTypes.UserName, login.UserName), new(SchoolClaimTypes.SecurityStamp, login.SecurityStamp),
            new(SchoolClaimTypes.PermissionsVersion, login.PermissionsVersion.ToString(System.Globalization.CultureInfo.InvariantCulture))
        };
        claims.AddRange(login.Roles.Select(x => new Claim(SchoolClaimTypes.Role, x)));
        claims.AddRange(login.Permissions.Select(x => new Claim(SchoolClaimTypes.Permission, x)));
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims), Issuer = options.Issuer, Audience = options.Audience,
            IssuedAt = now.UtcDateTime, NotBefore = now.UtcDateTime, Expires = expires.UtcDateTime,
            SigningCredentials = new SigningCredentials(options.SecurityKey(), SecurityAlgorithms.HmacSha256)
        };
        var handler = new JwtSecurityTokenHandler();
        return new SchoolAccessToken(handler.WriteToken(handler.CreateToken(descriptor)), options.AccessTokenMinutes * 60, expires);
    }
}

internal static class SchoolAuthenticationExtensions
{
    public static IServiceCollection AddSchoolAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = SchoolJwtOptions.FromConfiguration(configuration);
        services.AddSingleton(jwt); services.AddSingleton<ISchoolAccessTokenIssuer, SchoolAccessTokenIssuer>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = jwt.Issuer, ValidateAudience = true, ValidAudience = jwt.Audience,
                ValidateLifetime = true, RequireExpirationTime = true, ValidateIssuerSigningKey = true,
                IssuerSigningKey = jwt.SecurityKey(), ValidAlgorithms = [SecurityAlgorithms.HmacSha256], ClockSkew = TimeSpan.Zero,
                NameClaimType = JwtRegisteredClaimNames.Name, RoleClaimType = SchoolClaimTypes.Role
            };
        });
        services.AddAuthorization(options => options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser().Build());
        services.AddSingleton<IAuthorizationPolicyProvider, SchoolPermissionPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, SchoolPermissionHandler>();
        return services;
    }
}
