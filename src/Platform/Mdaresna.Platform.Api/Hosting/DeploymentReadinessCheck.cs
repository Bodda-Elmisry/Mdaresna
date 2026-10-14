using System.Data.Common;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Mdaresna.Platform.Api.Hosting;

// Connectivity only: schema migrations and broker delivery require separate deployment smoke tests.
internal sealed class DeploymentReadinessCheck(IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var cipher = Mdaresna.Platform.Api.Auth.UploadEncryptionConfiguration.Create(configuration);
            using (var encryptedProbe = new MemoryStream())
                await cipher.EncryptAsync(Stream.Null, encryptedProbe, "readiness", cancellationToken);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(3));
            foreach (var name in new[] { "PlatformConnection", "IdentityConnection" })
            {
                var value = configuration.GetConnectionString(name);
                if (string.IsNullOrWhiteSpace(value)) return HealthCheckResult.Unhealthy("Required database configuration is missing.");
                await using DbConnection connection = configuration["PlatformDatabase:Provider"] == "SqlServer"
                    ? new Microsoft.Data.SqlClient.SqlConnection(value)
                    : new Npgsql.NpgsqlConnection(value);
                await connection.OpenAsync(timeout.Token);
                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                command.CommandTimeout = 3;
                await command.ExecuteScalarAsync(timeout.Token);
            }
            return HealthCheckResult.Healthy();
        }
        catch
        {
            // Never return exceptions containing connection strings or key material.
            return HealthCheckResult.Unhealthy("Database connectivity or upload encryption configuration is not ready.");
        }
    }
}
