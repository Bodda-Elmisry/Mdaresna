using Mdaresna.SharedKernel;
using Mdaresna.Schools.Api.Documents;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Mdaresna.Schools.UnitTests;
public sealed class UploadedFileEncryptionTests
{
    private static UploadedFileCipher Cipher(string active = "one") => new(active, Keys);
    private static readonly Dictionary<string, string> Keys = new() { ["one"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)), ["two"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) };
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(65535)] [InlineData(65536)] [InlineData(140000)]
    public async Task Encrypted_roundtrip_is_seekable_and_keeps_original_hash_and_size(int size)
    {
        var bytes = RandomNumberGenerator.GetBytes(size); using var output = new MemoryStream();
        var result = await Cipher().EncryptAsync(new MemoryStream(bytes), output, "school/file", default);
        Assert.Equal(size, result.Size); Assert.Equal(Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(), result.Sha256);
        Assert.False(output.ToArray().SequenceEqual(bytes)); output.Position = 0;
        using var decoded = Cipher().OpenRead(output, "school/file"); Assert.Equal(size, decoded.Length);
        using var recovered = new MemoryStream(); await decoded.CopyToAsync(recovered); Assert.Equal(bytes, recovered.ToArray());
        if (size > 1) { decoded.Seek(size / 2, SeekOrigin.Begin); Assert.Equal(bytes[size / 2], decoded.ReadByte()); }
    }
    [Fact] public void Encryption_is_randomized_and_rotation_keeps_old_key_readable()
    {
        var data = Encoding.UTF8.GetBytes("private file"); var first = Cipher().EncryptBytes(data, "one/file"); var second = Cipher().EncryptBytes(data, "one/file"); Assert.NotEqual(first, second);
        using var stream = Cipher("two").OpenRead(new MemoryStream(first), "one/file"); Assert.Equal(data[0], stream.ReadByte());
    }
    [Fact] public void Missing_configuration_never_falls_back_to_plaintext_writes()
    {
        Assert.Throws<InvalidOperationException>(() => new UploadedFileCipher(null, new Dictionary<string,string>()).EncryptBytes([1], "file"));
        Assert.Throws<InvalidOperationException>(() => new UploadedFileCipher("one", new Dictionary<string,string>()).EncryptBytes([1], "file"));
    }
    [Fact] public void Scope_tampering_and_missing_keys_are_rejected()
    {
        var encoded = Cipher().EncryptBytes([1,2,3], "school-a/file");
        Assert.ThrowsAny<CryptographicException>(() => Cipher().OpenRead(new MemoryStream(encoded), "school-b/file"));
        Assert.Throws<InvalidOperationException>(() => new UploadedFileCipher("two", new Dictionary<string,string> { ["two"] = Keys["two"] }).OpenRead(new MemoryStream(encoded), "school-a/file"));
        encoded[104] ^= 1; Assert.ThrowsAny<CryptographicException>(() => Cipher().OpenRead(new MemoryStream(encoded), "school-a/file"));
    }
    [Fact] public void Damaged_chunk_and_truncation_are_rejected_before_returning_that_chunk()
    {
        var encoded = Cipher().EncryptBytes(new byte[70000], "file"); var truncated = encoded[..^1];
        Assert.Throws<CryptographicException>(() => Cipher().OpenRead(new MemoryStream(truncated), "file"));
        encoded[128] ^= 1; using var stream = Cipher().OpenRead(new MemoryStream(encoded), "file"); Assert.Throws<AuthenticationTagMismatchException>(() => stream.ReadByte());
    }
    [Fact] public void Legacy_images_are_readable_but_new_corrupted_envelopes_are_not_treated_as_images()
    {
        byte[] png = [137,80,78,71,13,10,26,10,0,0,0,0]; Assert.Equal(png, Cipher().DecryptImageBytes(png, "profile"));
        Assert.Throws<CryptographicException>(() => Cipher().DecryptImageBytes(png, "profile", false));
        var encoded = Cipher().EncryptBytes(png, "profile"); Assert.Equal(png, Cipher().DecryptImageBytes(encoded, "profile"));
        encoded[0] ^= 1; Assert.Throws<CryptographicException>(() => Cipher().DecryptImageBytes(encoded, "profile"));
    }
    [Fact] public void Server_watermark_is_baked_into_a_bounded_PNG()
    {
        var prefix = Encoding.ASCII.GetBytes("P6\n200 60\n255\n"); var ppm = prefix.Concat(Enumerable.Repeat((byte)255, 200*60*3)).ToArray();
        var clean = WatermarkedBookPage.FromPpm(ppm, ""); var marked = WatermarkedBookPage.FromPpm(ppm, "SCHOOL USER-01");
        Assert.Equal(new byte[] {137,80,78,71,13,10,26,10}, marked[..8]); Assert.NotEqual(clean, marked);
        Assert.Throws<InvalidDataException>(() => WatermarkedBookPage.FromPpm("P6\n99999 1\n255\n"u8.ToArray(), "user"));
        Assert.Throws<InvalidDataException>(() => WatermarkedBookPage.FromPpm(ppm[..^1], "user"));
    }
}
