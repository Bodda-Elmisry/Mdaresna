using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Mdaresna.Platform.Worker;

internal sealed class WorkerReadinessHealthCheck(WorkerReadinessState readinessState)
    : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var result = readinessState.IsReady
            ? HealthCheckResult.Healthy("The worker background service is running.")
            : HealthCheckResult.Unhealthy("The worker background service is not ready.");

        return Task.FromResult(result);
    }
}
