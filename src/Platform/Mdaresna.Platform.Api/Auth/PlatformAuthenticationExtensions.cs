using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Platform.Infrastructure.IdentityAuth;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Mdaresna.Platform.Api.Auth;

public static class PlatformAuthenticationExtensions
{
    public static IServiceCollection AddPlatformOperatorAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var jwt = PlatformJwtOptions.FromConfiguration(configuration);
        services.AddSingleton(jwt);
        services.AddScoped<IPasswordHasher<Account>, PasswordHasher<Account>>();
        services.AddScoped<PlatformPasswordCredentialFactory>();
        services.AddScoped<PlatformLoginService>();
        services.AddSingleton(_ => PlatformActivationOptions.FromConfiguration(configuration));
        services.AddScoped<PlatformFirstOwnerActivationService>();
        services.AddScoped<PlatformPasswordResetService>();
        services.AddScoped<PlatformPrincipalValidator>();
        services.AddSingleton<IPlatformAccessTokenIssuer, PlatformAccessTokenIssuer>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true,
                    RequireSignedTokens = true,
                    IssuerSigningKey = jwt.CreateSecurityKey(),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = JwtRegisteredClaimNames.Sub
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        if (context.Principal is null ||
                            !await context.HttpContext.RequestServices
                                .GetRequiredService<PlatformPrincipalValidator>()
                                .ValidateAsync(
                                    context.Principal,
                                    context.HttpContext.RequestAborted))
                        {
                            context.Fail("Platform account is not active or authorized.");
                        }
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder(
                    JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();
        });
        services.AddSingleton<IAuthorizationPolicyProvider, PlatformPermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PlatformPermissionHandler>();
        return services;
    }
}
