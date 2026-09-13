using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Mdaresna.Platform.Infrastructure.HealthChecks;

internal sealed class PostgreSqlConnectionHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public PostgreSqlConnectionHealthCheck(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Timeout = 3,
            CommandTimeout = 3
        };
        _connectionString = builder.ConnectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            return (int?)await command.ExecuteScalarAsync(cancellationToken) == 1
                ? HealthCheckResult.Healthy("PostgreSQL database is reachable.")
                : HealthCheckResult.Unhealthy("PostgreSQL database returned an unexpected result.");
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL database is unavailable.");
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL database health check timed out.");
        }
    }
}
