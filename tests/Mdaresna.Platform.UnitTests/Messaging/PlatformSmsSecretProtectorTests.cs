using System.Security.Cryptography;
using Mdaresna.Platform.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;

namespace Mdaresna.Platform.UnitTests.Messaging;

public sealed class PlatformSmsSecretProtectorTests
{
    [Fact]
    public void Protect_and_unprotect_round_trip_unicode_secret()
    {
        var protector = CreateProtector(RandomNumberGenerator.GetBytes(32));
        const string secret = "كلمة سر + & / 123";

        var encrypted = protector.Protect(secret);

        Assert.StartsWith("v1:", encrypted, StringComparison.Ordinal);
        Assert.DoesNotContain(secret, encrypted, StringComparison.Ordinal);
        Assert.Equal(secret, protector.Unprotect(encrypted));
    }

    [Fact]
    public void Repeated_encryption_uses_distinct_nonces()
    {
        var protector = CreateProtector(RandomNumberGenerator.GetBytes(32));

        var first = protector.Protect("provider-password");
        var second = protector.Protect("provider-password");

        Assert.NotEqual(first, second);
        Assert.Equal("provider-password", protector.Unprotect(first));
        Assert.Equal("provider-password", protector.Unprotect(second));
    }

    [Fact]
    public void Empty_provider_response_is_encrypted_and_distinct_from_no_response()
    {
        var protector = CreateProtector(RandomNumberGenerator.GetBytes(32));

        var encrypted = protector.ProtectPayload(string.Empty);

        Assert.StartsWith("v1:", encrypted, StringComparison.Ordinal);
        Assert.Equal(string.Empty, protector.Unprotect(encrypted));
    }

    [Fact]
    public void Wrong_key_cannot_decrypt_credential()
    {
        var firstKey = RandomNumberGenerator.GetBytes(32);
        var secondKey = RandomNumberGenerator.GetBytes(32);
        var encrypted = CreateProtector(firstKey).Protect("private-provider-password");

        var error = Assert.Throws<InvalidOperationException>(() =>
            CreateProtector(secondKey).Unprotect(encrypted));

        Assert.DoesNotContain("private-provider-password", error.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Missing_or_invalid_encryption_key_is_rejected()
    {
        var missing = new ConfigurationBuilder().Build();
        Assert.Throws<InvalidOperationException>(() => new PlatformSmsSecretProtector(missing));

        var tooShort = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PlatformSms:EncryptionKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16))
            })
            .Build();
        Assert.Throws<InvalidOperationException>(() => new PlatformSmsSecretProtector(tooShort));
    }

    private static PlatformSmsSecretProtector CreateProtector(byte[] key)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PlatformSms:EncryptionKey"] = Convert.ToBase64String(key)
            })
            .Build();
        return new PlatformSmsSecretProtector(configuration);
    }
}
