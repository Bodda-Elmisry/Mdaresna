using Mdaresna.Doamin.Enums;

namespace Mdarens.ReportingWorker;

public interface IReportingJob
{
    Task ProcessPendingReportsAsync(CancellationToken cancellationToken);
}
