using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace Mdaresna.Schools.Infrastructure.Identity;

public interface ISchoolOwnerActivationCodeSender
{
    Task<bool> SendAsync(Guid platformAccountId, Guid platformSchoolId, string fullLogin, string code,
        CancellationToken cancellationToken = default);
    Task<bool> ConfirmAsync(Guid platformAccountId, Guid platformSchoolId,
        CancellationToken cancellationToken = default);
}

internal sealed class PlatformSchoolOwnerActivationCodeSender(
    HttpClient client,
    IConfiguration configuration) : ISchoolOwnerActivationCodeSender
{
    public async Task<bool> SendAsync(Guid platformAccountId, Guid platformSchoolId, string fullLogin, string code,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = configuration["SchoolIdentity:PlatformBaseUrl"]?.TrimEnd('/');
        var apiKey = configuration["SchoolIdentity:InternalApiKey"];
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("SchoolIdentity PlatformBaseUrl and InternalApiKey are required.");
        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"{baseUrl}/api/platform/v1/internal/school-owner-activation-codes");
        request.Headers.Add("X-Mdaresna-Internal-Key", apiKey);
        request.Content = JsonContent.Create(new
        {
            PlatformAccountId = platformAccountId,
            PlatformSchoolId = platformSchoolId,
            FullLogin = fullLogin,
            Code = code
        });
        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    public async Task<bool> ConfirmAsync(Guid platformAccountId, Guid platformSchoolId,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = configuration["SchoolIdentity:PlatformBaseUrl"]?.TrimEnd('/');
        var apiKey = configuration["SchoolIdentity:InternalApiKey"];
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("SchoolIdentity PlatformBaseUrl and InternalApiKey are required.");
        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"{baseUrl}/api/platform/v1/internal/school-owner-activation-codes/confirmed");
        request.Headers.Add("X-Mdaresna-Internal-Key", apiKey);
        request.Content = JsonContent.Create(new { PlatformAccountId = platformAccountId, PlatformSchoolId = platformSchoolId });
        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException) { return false; }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { return false; }
    }
}
