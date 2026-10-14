using System.Net;
using System.Net.Http.Json;
using Mdaresna.Schools.Contracts.Provisioning;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Mdaresna.Schools.Infrastructure.Identity;

internal sealed class SchoolRoutingSnapshotClient(HttpClient client, IConfiguration config)
{
    public async Task<SchoolRoutingChangedV1?> ReadAsync(string code, CancellationToken ct)
    {
        var url = config["SchoolIdentity:PlatformBaseUrl"]?.TrimEnd('/') ?? throw new InvalidOperationException("Missing Platform URL.");
        using var request = new HttpRequestMessage(HttpMethod.Get, url + "/api/platform/v1/internal/school-routing-targets/" + Uri.EscapeDataString(code));
        request.Headers.Add("X-Mdaresna-Internal-Key", config["SchoolIdentity:InternalApiKey"]);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct); timeout.CancelAfter(TimeSpan.FromSeconds(5));
        using var response = await client.SendAsync(request, timeout.Token);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            var missing = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: timeout.Token);
            if (missing.ValueKind == JsonValueKind.Object && missing.TryGetProperty("code", out var codeValue) && codeValue.GetString() == "school_routing.not_found") return null;
            throw new InvalidOperationException("Routing endpoint is not deployed; retry without changing Redis.");
        }
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SchoolRoutingChangedV1>(cancellationToken: timeout.Token)
            ?? throw new InvalidOperationException("Empty routing snapshot.");
    }
}
