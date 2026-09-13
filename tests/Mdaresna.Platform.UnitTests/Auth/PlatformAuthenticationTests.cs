using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Infrastructure.IdentityAuth;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Mdaresna.Platform.UnitTests.Auth;

public sealed class PlatformAuthenticationTests
{
    private const string TestSecret =
        "unit-test-only-secret-00000000000000000000000000000000";

    [Fact]
    public void Auth_configuration_refuses_missing_signing_key()
    {
        var config = Configuration(new Dictionary<string, string?>
        {
            ["PlatformAuth:Issuer"] = "mdaresna-platform-test",
            ["PlatformAuth:Audience"] = "mdaresna-platform-api-test"
        });

        Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddPlatformOperatorAuthentication(config));
    }

    [Theory]
    [InlineData("short", null)]
    [InlineData(TestSecret, "0")]
    [InlineData(TestSecret, "16")]
    public void Auth_configuration_refuses_weak_key_or_out_of_range_lifetime(
        string secret,
        string? tokenMinutes)
    {
        var config = Configuration(new Dictionary<string, string?>
        {
            ["PlatformAuth:Issuer"] = "mdaresna-platform-test",
            ["PlatformAuth:Audience"] = "mdaresna-platform-api-test",
            ["PlatformAuth:SigningKey"] = secret,
            ["PlatformAuth:AccessTokenMinutes"] = tokenMinutes
        });

        Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddPlatformOperatorAuthentication(config));
    }

    [Fact]
    public void Issued_token_is_signed_short_lived_and_identifies_only_the_account()
    {
        var config = Configuration(new Dictionary<string, string?>
        {
            ["PlatformAuth:Issuer"] = "mdaresna-platform-test",
            ["PlatformAuth:Audience"] = "mdaresna-platform-api-test",
            ["PlatformAuth:SigningKey"] = TestSecret,
            ["PlatformAuth:AccessTokenMinutes"] = "10"
        });
        using var provider = new ServiceCollection()
            .AddPlatformOperatorAuthentication(config)
            .BuildServiceProvider();
        var issuer = provider.GetRequiredService<IPlatformAccessTokenIssuer>();
        var accountId = Guid.NewGuid();

        var issued = issuer.Issue(new PlatformLoginResult(
            accountId,
            "current-security-stamp",
            "Platform Operator"));

        Assert.Equal(600, issued.ExpiresInSeconds);
        Assert.InRange(issued.ExpiresAtUtc,
            DateTimeOffset.UtcNow.AddMinutes(9),
            DateTimeOffset.UtcNow.AddMinutes(11));

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var principal = handler.ValidateToken(
            issued.Token,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "mdaresna-platform-test",
                ValidateAudience = true,
                ValidAudience = "mdaresna-platform-api-test",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecret)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            },
            out _);

        Assert.Equal(accountId.ToString("D"),
            principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value);
        Assert.Equal(PlatformTokenClaims.TokenPurpose,
            principal.FindFirst(PlatformTokenClaims.Purpose)?.Value);
        Assert.Equal("current-security-stamp",
            principal.FindFirst(PlatformTokenClaims.SecurityStamp)?.Value);
        Assert.DoesNotContain(principal.Claims,
            claim => claim.Type.Contains("permission", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Provisioned_password_is_hashed_and_stamped()
    {
        var account = new Account { Id = Guid.NewGuid(), Status = AccountStatus.Active };
        var hasher = new PasswordHasher<Account>();
        var factory = new PlatformPasswordCredentialFactory(hasher);
        var now = DateTimeOffset.UtcNow;

        var credential = factory.Create(account, "long-example-password", now);

        Assert.NotEqual("long-example-password", credential.PasswordHash);
        Assert.Equal(PlatformPasswordCredentialFactory.Algorithm, credential.HashingAlgorithm);
        Assert.Equal(PlatformPasswordCredentialFactory.Version, credential.HashingVersion);
        Assert.False(string.IsNullOrWhiteSpace(credential.SecurityStamp));
        Assert.Equal(PasswordVerificationResult.Success,
            hasher.VerifyHashedPassword(
                account,
                credential.PasswordHash,
                "long-example-password"));
    }

    [Fact]
    public void Permission_attribute_uses_platform_permission_policy()
    {
        var attribute = new PlatformPermissionAttribute("platform.schools.manage");

        Assert.Equal("PlatformPermission:platform.schools.manage", attribute.Policy);
    }

    private static IConfiguration Configuration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();
}
