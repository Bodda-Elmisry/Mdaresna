using Mdarens.ReportingWorker.Entities;

namespace Mdarens.ReportingWorker.Services;

public class WorkerJobSettingsService : IWorkerJobSettingsService
{
    private readonly IConfiguration configuration;
    private readonly Dictionary<string, DateTime> lastRunByJobName = new();

    public WorkerJobSettingsService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public Task<WorkerJobSetting?> GetAsync(string jobName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var section = configuration.GetSection($"WorkerJobs:{jobName}");
        lastRunByJobName.TryGetValue(jobName, out var lastRunAt);

        var setting = new WorkerJobSetting
        {
            JobName = jobName,
            IsEnabled = section.GetValue("IsEnabled", true),
            RunsPerDay = section.GetValue("RunsPerDay", 1440),
            StartTime = section.GetValue("StartTime", TimeSpan.Zero),
            EndTime = section.GetValue("EndTime", TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59))),
            LastRunAt = lastRunAt == default ? null : lastRunAt,
            BatchSize = section.GetValue("BatchSize", 30),
            MaxItemsPerRun = section.GetValue("MaxItemsPerRun", 500)
        };

        return Task.FromResult<WorkerJobSetting?>(setting);
    }

    public Task MarkRunCompletedAsync(string jobName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lastRunByJobName[jobName] = DateTime.UtcNow;
        return Task.CompletedTask;
    }
}
