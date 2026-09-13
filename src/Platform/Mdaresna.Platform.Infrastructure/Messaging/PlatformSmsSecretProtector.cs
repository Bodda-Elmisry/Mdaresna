using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Mdaresna.Platform.Infrastructure.Messaging;

/// <summary>
/// Encrypts provider credentials at rest. The deployment-owned key is never
/// written to the Platform database or returned by the management API.
/// </summary>
public sealed class PlatformSmsSecretProtector
{
    private readonly byte[] _key;

    public PlatformSmsSecretProtector(IConfiguration configuration)
    {
        var configured = configuration["PlatformSms:EncryptionKey"];
        try
        {
            _key = Convert.FromBase64String(configured ?? string.Empty);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException(
                "PlatformSms:EncryptionKey must be a Base64-encoded random 32-byte key.");
        }

        if (_key.Length != 32)
        {
            throw new InvalidOperationException(
                "PlatformSms:EncryptionKey must be a Base64-encoded random 32-byte key.");
        }
    }

    public string Protect(string secret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);
        return ProtectPayload(secret);
    }

    /// <summary>
    /// Encrypts arbitrary SMS/audit text, including an empty gateway response.
    /// A null response means no response was received at all.
    /// </summary>
    public string ProtectPayload(string payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        var plaintext = Encoding.UTF8.GetBytes(payload);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[16];
        using var aes = new AesGcm(_key, tag.Length);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);
        var packed = new byte[nonce.Length + tag.Length + ciphertext.Length];
        nonce.CopyTo(packed, 0);
        tag.CopyTo(packed, nonce.Length);
        ciphertext.CopyTo(packed, nonce.Length + tag.Length);
        CryptographicOperations.ZeroMemory(plaintext);
        return "v1:" + Convert.ToBase64String(packed);
    }

    public string Unprotect(string encrypted)
    {
        if (!encrypted.StartsWith("v1:", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("SMS provider credential format is unsupported.");
        }

        byte[] packed;
        try
        {
            packed = Convert.FromBase64String(encrypted[3..]);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("SMS provider credential cannot be read.");
        }

        if (packed.Length < 28)
        {
            throw new InvalidOperationException("SMS provider credential cannot be read.");
        }

        var plaintext = new byte[packed.Length - 28];
        try
        {
            using var aes = new AesGcm(_key, 16);
            aes.Decrypt(packed.AsSpan(0, 12), packed.AsSpan(28),
                packed.AsSpan(12, 16), plaintext);
            return Encoding.UTF8.GetString(plaintext);
        }
        catch (CryptographicException)
        {
            throw new InvalidOperationException("SMS provider credential cannot be decrypted.");
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }
}
