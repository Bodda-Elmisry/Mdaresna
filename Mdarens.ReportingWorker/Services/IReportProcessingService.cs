namespace Mdarens.ReportingWorker.Services;

public interface IReportProcessingService
{
    Task ProcessPendingReportsAsync(CancellationToken cancellationToken);
}
