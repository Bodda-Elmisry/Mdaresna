using Mdarens.ReportingWorker.Entities;

namespace Mdarens.ReportingWorker.Services;

public interface IWorkerJobSettingsService
{
    Task<WorkerJobSetting?> GetAsync(string jobName, CancellationToken cancellationToken);

    Task MarkRunCompletedAsync(string jobName, CancellationToken cancellationToken);
}
