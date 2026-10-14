using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Mdaresna.Platform.Worker;

internal sealed class WorkerReadinessHealthCheck(WorkerReadinessState readinessState, IConfiguration configuration)
    : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!readinessState.IsReady)
            return HealthCheckResult.Unhealthy("The worker background service is not ready.");
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(3));
            foreach (var name in new[] { "PlatformConnection", "IdentityConnection" })
            {
                var value = configuration.GetConnectionString(name);
                if (string.IsNullOrWhiteSpace(value)) return HealthCheckResult.Unhealthy("Worker database configuration is missing.");
                await using System.Data.Common.DbConnection connection = configuration["PlatformDatabase:Provider"] == "SqlServer"
                    ? new Microsoft.Data.SqlClient.SqlConnection(value)
                    : new Npgsql.NpgsqlConnection(value);
                await connection.OpenAsync(timeout.Token);
                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                command.CommandTimeout = 3;
                await command.ExecuteScalarAsync(timeout.Token);
            }
            return HealthCheckResult.Healthy("Worker service and database connectivity are ready.");
        }
        catch
        {
            return HealthCheckResult.Unhealthy("Worker database connectivity is not ready.");
        }
    }
}
