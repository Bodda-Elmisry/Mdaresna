using System.Net.Http.Json;
using Mdaresna.Schools.Application.Identity;
using Microsoft.Extensions.Configuration;

namespace Mdaresna.Schools.Infrastructure.Identity;

internal sealed class PlatformSchoolLoginTenantResolver(HttpClient client, IConfiguration configuration)
    : ISchoolLoginTenantResolver
{
    public async Task<SchoolLoginTarget?> ResolveAsync(string schoolCode, CancellationToken cancellationToken = default)
    {
        var baseUrl = configuration["SchoolIdentity:PlatformBaseUrl"]?.TrimEnd('/');
        var apiKey = configuration["SchoolIdentity:InternalApiKey"];
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("SchoolIdentity PlatformBaseUrl and InternalApiKey are required.");

        using var request = new HttpRequestMessage(HttpMethod.Get,
            $"{baseUrl}/api/platform/v1/internal/school-login-targets/{Uri.EscapeDataString(schoolCode)}");
        request.Headers.Add("X-Mdaresna-Internal-Key", apiKey);
        using var response = await client.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        var target = await response.Content.ReadFromJsonAsync<TargetResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Platform returned an empty school login target.");
        var secret = ResolveSecret(target.CredentialSecretReference);
        return new SchoolLoginTarget(target.TenantId, target.SchoolId, target.SchoolCode,
            target.Provider, BuildConnectionString(target, secret));
    }

    private string ResolveSecret(string reference)
    {
        if (reference.StartsWith("env:", StringComparison.OrdinalIgnoreCase))
            return Environment.GetEnvironmentVariable(reference[4..])
                ?? throw new InvalidOperationException($"Database secret environment variable '{reference[4..]}' is missing.");
        return configuration[$"SchoolIdentity:DatabaseSecrets:{reference}"]
            ?? throw new InvalidOperationException($"Database secret '{reference}' is not configured.");
    }

    private static string BuildConnectionString(TargetResponse target, string secret)
    {
        if (secret.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
            secret.Contains("Server=", StringComparison.OrdinalIgnoreCase)) return secret;
        var tls = target.RequireTls ? "Require" : "Prefer";
        return target.Provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase)
            ? $"Host={target.Host};Port={target.Port};Database={target.DatabaseName};SSL Mode={tls};{secret}"
            : $"Server={target.Host},{target.Port};Database={target.DatabaseName};Encrypt={target.RequireTls};TrustServerCertificate={!target.RequireTls};{secret}";
    }

    private sealed record TargetResponse(Guid TenantId, Guid SchoolId, string SchoolCode, string Status,
        string Provider, string Host, int Port, string DatabaseName, string CredentialSecretReference, bool RequireTls);
}
