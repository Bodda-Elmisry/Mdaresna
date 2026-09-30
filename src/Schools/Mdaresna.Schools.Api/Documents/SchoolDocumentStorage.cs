using System.Security.Cryptography;

namespace Mdaresna.Schools.Api.Documents;

public sealed record StoredSchoolDocument(string StorageKey, string Sha256, long SizeBytes);
public interface ISchoolDocumentStorage
{
    Task<StoredSchoolDocument> SaveAsync(string schoolCode, Guid documentId, Guid versionId, Stream content, string extension, CancellationToken ct);
    Task<Stream?> OpenReadAsync(string storageKey, CancellationToken ct);
}

public sealed class LocalSchoolDocumentStorage(IConfiguration configuration, IWebHostEnvironment environment) : ISchoolDocumentStorage
{
    private readonly string _root = ResolveRoot(configuration["SchoolDocuments:StorageRoot"], environment.ContentRootPath);

    public async Task<StoredSchoolDocument> SaveAsync(string schoolCode, Guid documentId, Guid versionId, Stream content, string extension, CancellationToken ct)
    {
        var safeSchool = new string(schoolCode.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(safeSchool)) throw new InvalidOperationException("Invalid school storage scope.");
        var key = $"schools/{safeSchool}/documents/{documentId:N}/{versionId:N}{extension.ToLowerInvariant()}";
        var path = Resolve(key); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[81920]; long size = 0; int read;
        while ((read = await content.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
        { await output.WriteAsync(buffer.AsMemory(0, read), ct); hash.AppendData(buffer, 0, read); size += read; }
        return new(key, Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant(), size);
    }

    public Task<Stream?> OpenReadAsync(string storageKey, CancellationToken ct)
    {
        var path = Resolve(storageKey);
        return Task.FromResult<Stream?>(File.Exists(path) ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true) : null);
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
