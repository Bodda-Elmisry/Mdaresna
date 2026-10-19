using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Mdaresna.Schools.Application.Registration;
using Mdaresna.Schools.Contracts.Registration;
using Microsoft.Extensions.Configuration;

namespace Mdaresna.Schools.Infrastructure.Identity;

internal sealed class PlatformSchoolRegistrationCatalog(HttpClient client, IConfiguration config) : ISchoolRegistrationCatalog
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
    public async Task<RegistrationPlanOffer[]> PlansAsync(CancellationToken ct)
    {
        using var response = await SendAsync(HttpMethod.Get, "/plans", null, ct);
        return await response.Content.ReadFromJsonAsync<RegistrationPlanOffer[]>(Json, ct) ?? [];
    }
    public async Task<Guid> PrepareAsync(PrepareRegistrationOffer input, CancellationToken ct)
    {
        using var response = await SendAsync(HttpMethod.Post, "", input, ct);
        var data = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return data.GetProperty("offerId").GetGuid();
    }
    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string suffix, object? payload, CancellationToken ct)
    {
        var url = config["SchoolIdentity:PlatformBaseUrl"]?.TrimEnd('/') ?? throw new InvalidOperationException("Platform URL is required.");
        using var request = new HttpRequestMessage(method, url + "/api/platform/v1/internal/school-registration-offers" + suffix);
        request.Headers.Add("X-Mdaresna-Internal-Key", config["SchoolIdentity:InternalApiKey"]);
        if (payload != null) request.Content = JsonContent.Create(payload, options: Json);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct); timeout.CancelAfter(TimeSpan.FromSeconds(10));
        var response = await client.SendAsync(request, timeout.Token);
        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Conflict)
        { response.Dispose(); throw new ArgumentException("Registration details or selected plan prices changed. Refresh plans and review the request."); }
        if (!response.IsSuccessStatusCode) { response.Dispose(); throw new InvalidOperationException("Registration catalog is unavailable."); }
        return response;
    }
}
