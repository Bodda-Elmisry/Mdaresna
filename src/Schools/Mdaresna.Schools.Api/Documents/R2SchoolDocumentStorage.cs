using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using System.Net;
using Mdaresna.SharedKernel;

namespace Mdaresna.Schools.Api.Documents;

public static class SchoolDocumentStorageRegistration
{
    public static IServiceCollection AddSchoolDocumentStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["SchoolDocuments:Provider"] ?? "Local";
        if (provider.Equals("Local", StringComparison.OrdinalIgnoreCase))
            services.AddSingleton<ISchoolDocumentStorage, LocalSchoolDocumentStorage>();
        else if (provider.Equals("R2", StringComparison.OrdinalIgnoreCase))
        {
            // Validate explicitly: never silently fall back to a different storage location.
            var settings = R2StorageSettings.Load(configuration);
            services.AddSingleton(settings);
            services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(
                new BasicAWSCredentials(settings.AccessKeyId, settings.SecretAccessKey),
                new AmazonS3Config { ServiceURL = settings.Endpoint, AuthenticationRegion = "auto", ForcePathStyle = true,
                    Timeout = TimeSpan.FromSeconds(30), MaxErrorRetry = 2 }));
            services.AddSingleton<ISchoolDocumentStorage, R2SchoolDocumentStorage>();
        }
        else throw new InvalidOperationException("SchoolDocuments:Provider must be Local or R2.");
        return services;
    }
}

public sealed class R2StorageSettings
{
    public required string Endpoint { get; init; }
    public required string Bucket { get; init; }
    public required string AccessKeyId { get; init; }
    public required string SecretAccessKey { get; init; }
    public int MaxUploadMiB { get; init; } = 64;
    public int ConcurrentUploads { get; init; } = 2;

    public static R2StorageSettings Load(IConfiguration configuration)
    {
        string Required(string key) => !string.IsNullOrWhiteSpace(configuration["SchoolDocuments:R2:" + key])
            ? configuration["SchoolDocuments:R2:" + key]! : throw new InvalidOperationException("Missing SchoolDocuments:R2:" + key);
        var endpoint = Required("Endpoint");
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || uri.Scheme != "https" ||
            !uri.Host.EndsWith(".r2.cloudflarestorage.com", StringComparison.OrdinalIgnoreCase) ||
            !uri.IsDefaultPort || uri.AbsolutePath != "/" || uri.Query.Length != 0 || uri.Fragment.Length != 0 || uri.UserInfo.Length != 0)
            throw new InvalidOperationException("SchoolDocuments:R2:Endpoint must be the HTTPS R2 S3 account endpoint.");
        var bucket = Required("Bucket");
        if (bucket.Length is < 3 or > 63 || bucket[0] == '-' || bucket[^1] == '-' || !bucket.All(c => c is >= 'a' and <= 'z' or >= '0' and <= '9' or '-'))
            throw new InvalidOperationException("Invalid SchoolDocuments:R2:Bucket.");
        var max = configuration.GetValue("SchoolDocuments:R2:MaxUploadMiB", 64);
        var concurrency = configuration.GetValue("SchoolDocuments:R2:ConcurrentUploads", 2);
        if (max is < 1 or > 256 || concurrency is < 1 or > 8)
            throw new InvalidOperationException("R2 upload limits are outside the supported range.");
        return new() { Endpoint = endpoint, Bucket = bucket, AccessKeyId = Required("AccessKeyId"),
            SecretAccessKey = Required("SecretAccessKey"), MaxUploadMiB = max, ConcurrentUploads = concurrency };
    }
}

public sealed class R2SchoolDocumentStorage : ISchoolDocumentStorage, IDisposable
{
    private readonly IAmazonS3 _s3;
    private readonly R2StorageSettings _settings;
    private readonly UploadedFileCipher _cipher;
    private readonly bool _allowLegacy;
    private readonly SemaphoreSlim _uploads;
    private readonly SemaphoreSlim _reads = new(16);

    public R2SchoolDocumentStorage(IAmazonS3 s3, R2StorageSettings settings, IConfiguration configuration)
    {
        _s3 = s3; _settings = settings; _cipher = UploadEncryptionConfiguration.Create(configuration);
        _allowLegacy = configuration.GetValue("UploadEncryption:AllowLegacyRead", true);
        _uploads = new(settings.ConcurrentUploads);
    }

    public async Task<StoredSchoolDocument> SaveAsync(string schoolCode, Guid documentId, Guid versionId, Stream content, string extension, CancellationToken ct)
    {
        var safeSchool = new string(schoolCode.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        if (safeSchool.Length == 0 || extension.Length > 16 || !extension.All(c => char.IsAsciiLetterOrDigit(c) || c == '.'))
            throw new InvalidOperationException("Invalid school document storage scope.");
        var key = $"schools/{safeSchool}/documents/{documentId:N}/{versionId:N}{extension.ToLowerInvariant()}.enc";
        await _uploads.WaitAsync(ct);
        try
        {
            // Seekable, capped ciphertext staging: no plaintext temporary files, no unbounded buffering.
            using var encrypted = new BoundedUploadBuffer(_settings.MaxUploadMiB * 1024 * 1024);
            var result = await _cipher.EncryptAsync(content, encrypted, key, ct);
            encrypted.Position = 0;
            await _s3.PutObjectAsync(new PutObjectRequest { BucketName = _settings.Bucket, Key = key, InputStream = encrypted,
                ContentType = "application/octet-stream", AutoCloseStream = false, IfNoneMatch = "*",
                DisablePayloadSigning = true, DisableDefaultChecksumValidation = true }, ct);
            return new(key, result.Sha256, result.Size);
        }
        finally { _uploads.Release(); }
    }

    public async Task<Stream?> OpenReadAsync(string storageKey, CancellationToken ct)
    {
        ValidateKey(storageKey);
        GetObjectMetadataResponse metadata;
        await _reads.WaitAsync(ct);
        try
        {
            metadata = await _s3.GetObjectMetadataAsync(new GetObjectMetadataRequest { BucketName = _settings.Bucket, Key = storageKey }, ct);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound) { return null; }
        finally { _reads.Release(); }
        if (string.IsNullOrEmpty(metadata.ETag) || metadata.ContentLength < 0)
            throw new IOException("R2 object metadata is incomplete.");
        var stream = new R2RangeStream(metadata.ContentLength, async (offset, count, token) =>
        {
            await _reads.WaitAsync(token).ConfigureAwait(false);
            try
            {
                using var response = await _s3.GetObjectAsync(new GetObjectRequest { BucketName = _settings.Bucket, Key = storageKey,
                    ByteRange = new ByteRange(offset, offset + count - 1), EtagToMatch = metadata.ETag }, token).ConfigureAwait(false);
                if (response.HttpStatusCode != HttpStatusCode.PartialContent || response.ContentLength != count || response.ETag != metadata.ETag ||
                    response.ContentRange != $"bytes {offset}-{offset + count - 1}/{metadata.ContentLength}")
                    throw new IOException("R2 returned an inconsistent object range.");
                var bytes = new byte[count];
                await response.ResponseStream.ReadExactlyAsync(bytes, token).ConfigureAwait(false);
                return bytes;
            }
            finally { _reads.Release(); }
        }, ct);
        try
        {
            await stream.PrefetchAsync(ct);
            return _cipher.OpenRead(stream, storageKey, _allowLegacy && !storageKey.EndsWith(".enc", StringComparison.OrdinalIgnoreCase));
        }
        catch { stream.Dispose(); throw; }
    }

    public async Task DeleteAsync(string storageKey, CancellationToken ct)
    {
        ValidateKey(storageKey);
        await _s3.DeleteObjectAsync(new DeleteObjectRequest { BucketName = _settings.Bucket, Key = storageKey }, ct);
    }

    // Read-only connectivity probe. Actual write/delete permissions are checked by the explicit smoke test.
    public async Task CheckReadinessAsync(CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(3));
        await _s3.ListObjectsV2Async(new ListObjectsV2Request { BucketName = _settings.Bucket, Prefix = ".readiness/", MaxKeys = 1 }, timeout.Token);
    }

    private static void ValidateKey(string key)
    {
        if (!key.StartsWith("schools/", StringComparison.Ordinal) || key.Contains('\\') || key.Split('/').Any(s => s is "" or "." or ".."))
            throw new InvalidOperationException("Invalid school document storage key.");
    }
    public void Dispose() { _uploads.Dispose(); _reads.Dispose(); }

    private sealed class BoundedUploadBuffer(int maximum) : MemoryStream
    {
        public override int Capacity { get => base.Capacity; set => base.Capacity = Math.Min(value, maximum); }
        private void Check(int count) { if (Position + count > maximum) throw new IOException("R2 encrypted upload exceeds configured staging limit."); }
        public override void Write(byte[] buffer, int offset, int count) { Check(count); base.Write(buffer, offset, count); }
        public override void Write(ReadOnlySpan<byte> buffer) { Check(buffer.Length); base.Write(buffer); }
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct = default) { Check(buffer.Length); return base.WriteAsync(buffer, ct); }
    }
}
