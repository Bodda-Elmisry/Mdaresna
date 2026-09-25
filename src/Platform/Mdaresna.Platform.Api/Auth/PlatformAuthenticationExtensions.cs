using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Platform.Infrastructure.IdentityAuth;
using Mdaresna.Platform.Api.Realtime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
        services.AddScoped<PlatformLoginService>();
        services.AddScoped<PlatformSessionService>();
        services.AddScoped<AccountAppLanguageService>();
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
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (string.IsNullOrEmpty(context.Token) &&
                            !string.IsNullOrEmpty(accessToken) &&
                            context.HttpContext.Request.Path.StartsWithSegments(
                                PlatformNotificationHub.Path))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },
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
