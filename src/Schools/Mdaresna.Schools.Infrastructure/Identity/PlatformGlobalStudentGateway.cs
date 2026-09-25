using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace Mdaresna.Schools.Infrastructure.Identity;

public sealed record GlobalStudentIdentity(Guid GlobalStudentId, string StudentCode);
public interface IGlobalStudentGateway
{
    Task<GlobalStudentIdentity?> ResolveOrCreateAsync(Guid platformSchoolId, string? studentCode, string fullName,
        DateOnly dateOfBirth, string? nationalId, string? birthCertificateNumber, CancellationToken ct = default);
}

internal sealed class PlatformGlobalStudentGateway(HttpClient client, IConfiguration configuration) : IGlobalStudentGateway
{
    public async Task<GlobalStudentIdentity?> ResolveOrCreateAsync(Guid platformSchoolId, string? studentCode,
        string fullName, DateOnly dateOfBirth, string? nationalId, string? birthCertificateNumber, CancellationToken ct = default)
    {
        var baseUrl = configuration["SchoolIdentity:PlatformBaseUrl"]?.TrimEnd('/');
        var apiKey = configuration["SchoolIdentity:InternalApiKey"];
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("SchoolIdentity PlatformBaseUrl and InternalApiKey are required.");
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/platform/v1/internal/global-students/resolve-or-create");
        request.Headers.Add("X-Mdaresna-Internal-Key", apiKey);
        request.Content = JsonContent.Create(new { platformSchoolId, studentCode, fullName, dateOfBirth, nationalId, birthCertificateNumber });
        try
        {
            using var response = await client.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return null;
            var envelope = await response.Content.ReadFromJsonAsync<Envelope<GlobalStudentIdentity>>(cancellationToken: ct);
            return envelope?.IsSuccess == true ? envelope.Data : null;
        }
        catch (HttpRequestException) { return null; }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { return null; }
    }
    private sealed record Envelope<T>(bool IsSuccess, T? Data);
}
