using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Mdaresna.Platform.Infrastructure.HealthChecks;

internal sealed class SqlConnectionHealthCheck : IHealthCheck
{
    private const int MaximumConnectTimeoutSeconds = 3;
    private readonly string _connectionString;

    public SqlConnectionHealthCheck(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            ConnectRetryCount = 0,
            ConnectTimeout = GetBoundedConnectTimeout(connectionString)
        };

        _connectionString = builder.ConnectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT CAST(1 AS int);";
            command.CommandTimeout = MaximumConnectTimeoutSeconds;
            var result = await command.ExecuteScalarAsync(cancellationToken);

            return result is 1
                ? HealthCheckResult.Healthy("SQL database is reachable.")
                : HealthCheckResult.Unhealthy("SQL database returned an unexpected result.");
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("SQL database is unavailable.");
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("SQL database health check timed out.");
        }
    }

    private static int GetBoundedConnectTimeout(string connectionString)
    {
        var configured = new SqlConnectionStringBuilder(connectionString).ConnectTimeout;
        return configured <= 0
            ? MaximumConnectTimeoutSeconds
            : Math.Min(configured, MaximumConnectTimeoutSeconds);
    }
}
