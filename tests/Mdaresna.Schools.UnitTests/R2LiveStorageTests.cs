using Amazon.S3;
using Mdaresna.Schools.Api.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using Xunit;

namespace Mdaresna.Schools.UnitTests;

public sealed class R2LiveStorageTests
{
    [R2LiveFact]
    public async Task Private_R2_encrypted_upload_seek_download_delete()
    {
        // Explicit opt-in only; no credentials in test arguments or test output.
        var config = new ConfigurationBuilder().AddUserSecrets(typeof(UploadEncryptionConfiguration).Assembly, optional: true)
            .AddEnvironmentVariables().AddInMemoryCollection(new Dictionary<string, string?> {
                ["SchoolDocuments:Provider"] = "R2",
                ["UploadEncryption:ActiveKeyId"] = "smoke",
                ["UploadEncryption:Keys:smoke"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
                ["UploadEncryption:AllowLegacyRead"] = "false" }).Build();
        var settings = R2StorageSettings.Load(config);
        Assert.True(settings.Bucket.EndsWith("-dev") || settings.Bucket.EndsWith("-test"), "Use a dedicated -dev or -test bucket for the live smoke test.");
        using var services = new ServiceCollection().AddSingleton<IConfiguration>(config).AddSchoolDocumentStorage(config).BuildServiceProvider();
        var storage = (R2SchoolDocumentStorage)services.GetRequiredService<ISchoolDocumentStorage>();
        var school = "r2smoke" + Guid.NewGuid().ToString("N");
        var document = Guid.NewGuid(); var version = Guid.NewGuid();
        var key = $"schools/{school}/documents/{document:N}/{version:N}.bin.enc";
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            var ct = timeout.Token;
            await storage.CheckReadinessAsync(ct);
            var plain = RandomNumberGenerator.GetBytes(900000);
            var result = await storage.SaveAsync(school, document, version, new MemoryStream(plain), ".bin", ct);
            Assert.Equal(key, result.StorageKey);
            using (var read = (await storage.OpenReadAsync(key, ct))!)
            {
                read.Position = 800000; var tail = new byte[1000]; await read.ReadExactlyAsync(tail, ct);
                Assert.Equal(plain[800000..801000], tail);
                read.Position = 0; using var recovered = new MemoryStream(); await read.CopyToAsync(recovered, ct);
                Assert.Equal(SHA256.HashData(plain), SHA256.HashData(recovered.ToArray()));
            }
            await storage.DeleteAsync(key, ct);
            Assert.Null(await storage.OpenReadAsync(key, ct));
        }
        catch (AmazonS3Exception ex)
        {
            throw new InvalidOperationException($"R2 live smoke failed with HTTP {(int)ex.StatusCode}. No credential values are included.");
        }
        finally
        {
            // Clean up only the unique object owned by this test, including an ambiguous upload timeout.
            using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            try { await storage.DeleteAsync(key, cleanup.Token); }
            catch { throw new InvalidOperationException("R2 smoke cleanup failed. Remove the test's r2smoke school prefix from the dev bucket before retrying."); }
        }
    }
}

public sealed class R2LiveFactAttribute : FactAttribute
{
    public R2LiveFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("MDARESNA_R2_LIVE_TEST") != "1")
            Skip = "Requires explicit opt-in and R2 development credentials stored locally.";
    }
}
