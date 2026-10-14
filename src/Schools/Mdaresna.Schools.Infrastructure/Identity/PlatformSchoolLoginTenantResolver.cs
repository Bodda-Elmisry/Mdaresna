using System.Net.Http.Json;
using Mdaresna.Schools.Application.Identity;
using Microsoft.Extensions.Configuration;
using Mdaresna.Schools.Contracts.Provisioning;

namespace Mdaresna.Schools.Infrastructure.Identity;

internal sealed class PlatformSchoolLoginTenantResolver(HttpClient client, IConfiguration configuration, SchoolRoutingCache cache)
    : ISchoolLoginTenantResolver
{
    public async Task<SchoolLoginTarget?> ResolveAsync(string schoolCode, CancellationToken cancellationToken = default)
    {
        schoolCode = schoolCode.Trim().ToUpperInvariant();
        var target = await cache.ResolveAsync(schoolCode, ct => LoadAsync(schoolCode, ct), cancellationToken);
        if (target is null) return null;
        var secret = ResolveSecret(target.CredentialSecretReference);
        return new SchoolLoginTarget(target.TenantId, target.SchoolId, target.SchoolCode,
            target.Provider, BuildConnectionString(target, secret));
    }

    private async Task<SchoolRoutingTarget?> LoadAsync(string schoolCode, CancellationToken cancellationToken)
    {
        var baseUrl = configuration["SchoolIdentity:PlatformBaseUrl"]?.TrimEnd('/');
        var apiKey = configuration["SchoolIdentity:InternalApiKey"];
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("SchoolIdentity PlatformBaseUrl and InternalApiKey are required.");

        using var request = new HttpRequestMessage(HttpMethod.Get,
            $"{baseUrl}/api/platform/v1/internal/school-routing-targets/{Uri.EscapeDataString(schoolCode)}");
        request.Headers.Add("X-Mdaresna-Internal-Key", apiKey);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        using var response = await client.SendAsync(request, timeout.Token);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        var snapshot = await response.Content.ReadFromJsonAsync<SchoolRoutingChangedV1>(cancellationToken: timeout.Token)
            ?? throw new InvalidOperationException("Platform returned an empty school login target.");
        Messaging.SchoolRoutingCacheConsumerService.Validate(snapshot);
        var data = snapshot.Target;
        if (data is null) return null;
        var target = new SchoolRoutingTarget(data.TenantId, data.SchoolId, data.SchoolCode, data.Status,
            data.Provider, data.Host, data.Port, data.DatabaseName, data.CredentialSecretReference, data.RequireTls, snapshot.RoutingVersion);
        if (target.Status != "Active" || target.SchoolCode != schoolCode)
            throw new InvalidOperationException("Platform returned an invalid school login target.");
        return target;
    }

    private string ResolveSecret(string reference)
    {
        if (reference.StartsWith("env:", StringComparison.OrdinalIgnoreCase))
            return Environment.GetEnvironmentVariable(reference[4..])
                ?? throw new InvalidOperationException($"Database secret environment variable '{reference[4..]}' is missing.");
        return configuration[$"SchoolIdentity:DatabaseSecrets:{reference}"]
            ?? throw new InvalidOperationException($"Database secret '{reference}' is not configured.");
    }

    private static string BuildConnectionString(SchoolRoutingTarget target, string secret)
    {
        if (secret.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
            secret.Contains("Server=", StringComparison.OrdinalIgnoreCase)) return secret;
        var tls = target.RequireTls ? "Require" : "Prefer";
        return target.Provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase)
            ? $"Host={target.Host};Port={target.Port};Database={target.DatabaseName};SSL Mode={tls};{secret}"
            : $"Server={target.Host},{target.Port};Database={target.DatabaseName};Encrypt={target.RequireTls};TrustServerCertificate={!target.RequireTls};{secret}";
    }

}
