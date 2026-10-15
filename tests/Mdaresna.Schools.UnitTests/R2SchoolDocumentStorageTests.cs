using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Mdaresna.Schools.Api.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Security.Cryptography;
using Xunit;

namespace Mdaresna.Schools.UnitTests;

public sealed class R2SchoolDocumentStorageTests
{
    private static IConfiguration Config(params (string Key, string? Value)[] overrides)
    {
        var values = new Dictionary<string, string?> {
            ["SchoolDocuments:R2:Endpoint"] = "https://test.r2.cloudflarestorage.com",
            ["SchoolDocuments:R2:Bucket"] = "schools-test",
            ["SchoolDocuments:R2:AccessKeyId"] = "test-key",
            ["SchoolDocuments:R2:SecretAccessKey"] = "test-secret",
            ["UploadEncryption:ActiveKeyId"] = "test",
            ["UploadEncryption:Keys:test"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) };
        foreach (var (key, value) in overrides) values[key] = value;
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(65536)] [InlineData(900000)]
    public async Task Ciphertext_roundtrip_and_random_seek_use_bounded_pinned_ranges(int size)
    {
        var config = Config(); using var client = new FakeS3();
        using var storage = new R2SchoolDocumentStorage(client, R2StorageSettings.Load(config), config);
        var original = RandomNumberGenerator.GetBytes(size);
        var saved = await storage.SaveAsync("SCH-1", Guid.NewGuid(), Guid.NewGuid(), new MemoryStream(original), ".pdf", default);
        Assert.Equal(size, saved.SizeBytes);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(original)).ToLowerInvariant(), saved.Sha256);
        Assert.False(client.Objects[saved.StorageKey].SequenceEqual(original));
        Assert.Equal("MDAENC01", System.Text.Encoding.ASCII.GetString(client.Objects[saved.StorageKey], 0, 8));
        Assert.True(client.PutCompatible);
        using var read = (await storage.OpenReadAsync(saved.StorageKey, default))!;
        Assert.True(read.CanSeek); Assert.Equal(size, read.Length);
        if (size > 800000)
        {
            // Header prefetch reads one page; seeking near the end must skip intervening pages.
            Assert.Single(client.Ranges);
            read.Position = 800000; var sample = new byte[100]; await read.ReadExactlyAsync(sample);
            Assert.Equal(original[800000..800100], sample);
            Assert.Equal(2, client.Ranges.Count);
        }
        read.Position = 0; using var recovered = new MemoryStream(); await read.CopyToAsync(recovered);
        Assert.Equal(original, recovered.ToArray());
        Assert.All(client.Ranges, range => Assert.InRange(range.End - range.Start + 1, 1, 256 * 1024));
        read.Position = size; Assert.Equal(-1, read.ReadByte());
        await storage.DeleteAsync(saved.StorageKey, default);
        Assert.Null(await storage.OpenReadAsync(saved.StorageKey, default));
    }

    [Fact]
    public async Task Corruption_or_changed_revision_fails_instead_of_returning_other_content()
    {
        var config = Config(); using var client = new FakeS3();
        using var storage = new R2SchoolDocumentStorage(client, R2StorageSettings.Load(config), config);
        var saved = await storage.SaveAsync("school", Guid.NewGuid(), Guid.NewGuid(), new MemoryStream(new byte[900000]), ".mp4", default);
        using (var read = (await storage.OpenReadAsync(saved.StorageKey, default))!)
        {
            client.Revision = "changed"; read.Position = 800000;
            await Assert.ThrowsAsync<AmazonS3Exception>(async () => await read.ReadAsync(new byte[10]));
        }
        client.Revision = "revision"; client.Objects[saved.StorageKey][128] ^= 1;
        using var corrupted = (await storage.OpenReadAsync(saved.StorageKey, default))!;
        await Assert.ThrowsAnyAsync<CryptographicException>(async () => await corrupted.ReadAsync(new byte[10]));
    }

    [Fact]
    public async Task Missing_key_oversize_and_cancel_never_upload_plaintext_or_partial_objects()
    {
        var config = Config(("SchoolDocuments:R2:MaxUploadMiB", "1")); using var client = new FakeS3();
        using var storage = new R2SchoolDocumentStorage(client, R2StorageSettings.Load(config), config);
        await Assert.ThrowsAsync<IOException>(() => storage.SaveAsync("school", Guid.NewGuid(), Guid.NewGuid(), new MemoryStream(new byte[2 * 1024 * 1024]), ".bin", default));
        using var cancel = new CancellationTokenSource(); cancel.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => storage.SaveAsync("school", Guid.NewGuid(), Guid.NewGuid(), Stream.Null, ".pdf", cancel.Token));
        var noKey = Config(("UploadEncryption:Keys:test", null));
        using var missing = new R2SchoolDocumentStorage(client, R2StorageSettings.Load(noKey), noKey);
        await Assert.ThrowsAsync<InvalidOperationException>(() => missing.SaveAsync("school", Guid.NewGuid(), Guid.NewGuid(), Stream.Null, ".pdf", default));
        Assert.Empty(client.Objects);
    }

    [Theory]
    [InlineData("http://test.r2.cloudflarestorage.com")]
    [InlineData("https://example.com")]
    [InlineData("https://test.r2.cloudflarestorage.com/bucket")]
    [InlineData("https://test.r2.cloudflarestorage.com?secret=value")]
    public void Invalid_endpoint_rejected_without_echoing_value(string endpoint)
    {
        var error = Assert.Throws<InvalidOperationException>(() => R2StorageSettings.Load(Config(("SchoolDocuments:R2:Endpoint", endpoint))));
        Assert.DoesNotContain(endpoint, error.Message);
    }

    [Fact]
    public void Provider_and_credentials_are_validated_without_local_fallback()
    {
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddSchoolDocumentStorage(Config(("SchoolDocuments:Provider", "typo"))));
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddSchoolDocumentStorage(Config(("SchoolDocuments:Provider", "R2"), ("SchoolDocuments:R2:SecretAccessKey", null))));
        var valid = Config(("SchoolDocuments:Provider", "R2"));
        using var services = new ServiceCollection().AddSingleton<IConfiguration>(valid).AddSchoolDocumentStorage(valid).BuildServiceProvider();
        Assert.IsType<R2SchoolDocumentStorage>(services.GetRequiredService<ISchoolDocumentStorage>());
    }

    [Fact]
    public async Task Forbidden_and_network_failures_are_not_reported_as_missing_files()
    {
        var config = Config(); using var client = new FakeS3 { MetadataError = HttpStatusCode.Forbidden };
        using var storage = new R2SchoolDocumentStorage(client, R2StorageSettings.Load(config), config);
        await Assert.ThrowsAsync<AmazonS3Exception>(() => storage.OpenReadAsync("schools/a/file.enc", default));
        client.MetadataError = HttpStatusCode.ServiceUnavailable;
        await Assert.ThrowsAsync<AmazonS3Exception>(() => storage.OpenReadAsync("schools/a/file.enc", default));
        await Assert.ThrowsAsync<InvalidOperationException>(() => storage.DeleteAsync("schools/../file", default));
        Assert.Empty(client.Objects);
    }

    [Fact]
    public async Task Range_stream_rejects_truncation_and_honors_cancellation()
    {
        using var stream = new R2RangeStream(100, (_, _, _) => Task.FromResult(new byte[2]), default);
        await Assert.ThrowsAsync<EndOfStreamException>(async () => await stream.ReadAsync(new byte[5]));
        using var cancel = new CancellationTokenSource(); cancel.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await stream.ReadAsync(new byte[5], cancel.Token));
        Assert.Throws<IOException>(() => stream.Seek(-1, SeekOrigin.Begin));
    }

    private sealed class FakeS3() : AmazonS3Client(new AnonymousAWSCredentials(), new AmazonS3Config { ServiceURL = "https://example.invalid" })
    {
        public Dictionary<string, byte[]> Objects { get; } = new();
        public List<(long Start, long End)> Ranges { get; } = new();
        public string Revision { get; set; } = "revision";
        public HttpStatusCode? MetadataError { get; set; }
        public bool PutCompatible { get; private set; }
        public override async Task<PutObjectResponse> PutObjectAsync(PutObjectRequest request, CancellationToken cancellationToken = default)
        {
            PutCompatible = request.DisablePayloadSigning == true && request.DisableDefaultChecksumValidation == true && request.ContentType == "application/octet-stream";
            Assert.Equal("*", request.IfNoneMatch);
            if (Objects.ContainsKey(request.Key)) throw new AmazonS3Exception("test", ErrorType.Sender, "PreconditionFailed", "test", HttpStatusCode.PreconditionFailed);
            using var bytes = new MemoryStream(); await request.InputStream.CopyToAsync(bytes, cancellationToken);
            Objects[request.Key] = bytes.ToArray(); return new();
        }
        public override Task<GetObjectMetadataResponse> GetObjectMetadataAsync(GetObjectMetadataRequest request, CancellationToken cancellationToken = default)
        {
            if (MetadataError.HasValue) throw new AmazonS3Exception("test", ErrorType.Sender, "test", "test", MetadataError.Value);
            if (!Objects.TryGetValue(request.Key, out var bytes)) throw new AmazonS3Exception("test", ErrorType.Sender, "NoSuchKey", "test", HttpStatusCode.NotFound);
            return Task.FromResult(new GetObjectMetadataResponse { ContentLength = bytes.Length, ETag = Revision });
        }
        public override Task<GetObjectResponse> GetObjectAsync(GetObjectRequest request, CancellationToken cancellationToken = default)
        {
            if (request.EtagToMatch != Revision) throw new AmazonS3Exception("test", ErrorType.Sender, "PreconditionFailed", "test", HttpStatusCode.PreconditionFailed);
            Ranges.Add((request.ByteRange.Start, request.ByteRange.End));
            var bytes = Objects[request.Key][(int)request.ByteRange.Start..((int)request.ByteRange.End + 1)];
            return Task.FromResult(new GetObjectResponse { HttpStatusCode = HttpStatusCode.PartialContent, ContentLength = bytes.Length,
                ETag = Revision, ContentRange = $"bytes {request.ByteRange.Start}-{request.ByteRange.End}/{Objects[request.Key].Length}", ResponseStream = new MemoryStream(bytes) });
        }
        public override Task<DeleteObjectResponse> DeleteObjectAsync(DeleteObjectRequest request, CancellationToken cancellationToken = default)
        { Objects.Remove(request.Key); return Task.FromResult(new DeleteObjectResponse()); }
    }
}
