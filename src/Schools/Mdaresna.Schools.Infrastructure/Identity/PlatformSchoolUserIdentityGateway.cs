using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace Mdaresna.Schools.Infrastructure.Identity;

public sealed record SchoolUserIdentity(Guid AccountId, string DisplayName, string PrimaryPhone, bool PhoneVerified);
public sealed record SchoolUserPrimaryContact(Guid Id, string PrimaryPhone, bool PhoneVerified);

public interface ISchoolUserIdentityGateway
{
    Task<SchoolUserIdentity?> ResolveAsync(Guid platformSchoolId, string phone, string displayName,
        CancellationToken cancellationToken = default);
    Task<bool> SendInvitationAsync(Guid platformSchoolId, Guid platformAccountId, string fullLogin,
        CancellationToken cancellationToken = default);
    Task<SchoolUserPrimaryContact?> GetPrimaryContactAsync(Guid platformSchoolId, Guid platformAccountId,
        CancellationToken cancellationToken = default);
}

internal sealed class PlatformSchoolUserIdentityGateway(HttpClient client, IConfiguration configuration)
    : ISchoolUserIdentityGateway
{
    public async Task<SchoolUserIdentity?> ResolveAsync(Guid platformSchoolId, string phone, string displayName,
        CancellationToken cancellationToken = default)
    {
        using var request = Create(HttpMethod.Post, "resolve", new { platformSchoolId, phone, displayName });
        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<SchoolUserIdentity>>(cancellationToken);
        return envelope?.IsSuccess == true ? envelope.Data : null;
    }

    public async Task<bool> SendInvitationAsync(Guid platformSchoolId, Guid platformAccountId, string fullLogin,
        CancellationToken cancellationToken = default)
    {
        using var request = Create(HttpMethod.Post, "send-invitation", new { platformSchoolId, platformAccountId, fullLogin });
        try { using var response = await client.SendAsync(request, cancellationToken); return response.IsSuccessStatusCode; }
        catch (HttpRequestException) { return false; }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { return false; }
    }

    public async Task<SchoolUserPrimaryContact?> GetPrimaryContactAsync(Guid platformSchoolId, Guid platformAccountId,
        CancellationToken cancellationToken = default)
    {
        using var request = Create(HttpMethod.Post, "primary-contact", new { platformSchoolId, platformAccountId });
        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            var envelope = await response.Content.ReadFromJsonAsync<Envelope<SchoolUserPrimaryContact>>(cancellationToken);
            return envelope?.IsSuccess == true ? envelope.Data : null;
        }
        catch (HttpRequestException) { return null; }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { return null; }
    }

    private HttpRequestMessage Create(HttpMethod method, string action, object body)
    {
        var baseUrl = configuration["SchoolIdentity:PlatformBaseUrl"]?.TrimEnd('/');
        var apiKey = configuration["SchoolIdentity:InternalApiKey"];
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("SchoolIdentity PlatformBaseUrl and InternalApiKey are required.");
        var request = new HttpRequestMessage(method, $"{baseUrl}/api/platform/v1/internal/school-user-identities/{action}");
        request.Headers.Add("X-Mdaresna-Internal-Key", apiKey);
        request.Content = JsonContent.Create(body);
        return request;
    }

    private sealed record Envelope<T>(bool IsSuccess, T? Data);
}
