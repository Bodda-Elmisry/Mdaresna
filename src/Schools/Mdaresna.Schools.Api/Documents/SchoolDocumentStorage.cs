using Mdaresna.SharedKernel;

namespace Mdaresna.Schools.Api.Documents;

public sealed record StoredSchoolDocument(string StorageKey, string Sha256, long SizeBytes);
public interface ISchoolDocumentStorage
{
    Task<StoredSchoolDocument> SaveAsync(string schoolCode, Guid documentId, Guid versionId, Stream content, string extension, CancellationToken ct);
    Task<Stream?> OpenReadAsync(string storageKey, CancellationToken ct);
    Task DeleteAsync(string storageKey, CancellationToken ct);
}

public sealed class LocalSchoolDocumentStorage(IConfiguration configuration, IWebHostEnvironment environment) : ISchoolDocumentStorage
{
    private readonly string _root = ResolveRoot(configuration["SchoolDocuments:StorageRoot"], environment.ContentRootPath);

    private readonly UploadedFileCipher _cipher = UploadEncryptionConfiguration.Create(configuration);

    public async Task<StoredSchoolDocument> SaveAsync(string schoolCode, Guid documentId, Guid versionId, Stream content, string extension, CancellationToken ct)
    {
        var safeSchool = new string(schoolCode.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(safeSchool)) throw new InvalidOperationException("Invalid school storage scope.");
        var key = $"schools/{safeSchool}/documents/{documentId:N}/{versionId:N}{extension.ToLowerInvariant()}.enc";
        var path = Resolve(key); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
        try {
            var result = await _cipher.EncryptAsync(content, output, key, ct);
            return new(key, result.Sha256, result.Size);
        }
        catch {
            await output.DisposeAsync();
            File.Delete(path);
            throw;
        }
    }

    public Task<Stream?> OpenReadAsync(string storageKey, CancellationToken ct)
    {
        var path = Resolve(storageKey);
        if (!File.Exists(path)) return Task.FromResult<Stream?>(null);
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        try { return Task.FromResult<Stream?>(_cipher.OpenRead(stream, storageKey, configuration.GetValue("UploadEncryption:AllowLegacyRead", true) && !storageKey.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))); }
        catch { stream.Dispose(); throw; }
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct)
    {
        File.Delete(Resolve(storageKey));
        return Task.CompletedTask;
    }

    private string Resolve(string key)
    {
        var path = Path.GetFullPath(Path.Combine(_root, key.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Invalid storage key.");
        return path;
    }

    private static string ResolveRoot(string? configured, string contentRoot)
    {
        var value = string.IsNullOrWhiteSpace(configured) ? Path.Combine("App_Data", "school-documents") : configured;
        return Path.GetFullPath(Path.IsPathRooted(value) ? value : Path.Combine(contentRoot, value));
    }
}
