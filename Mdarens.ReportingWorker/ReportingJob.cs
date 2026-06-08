using Hangfire;
using Mdarens.ReportingWorker.Services;
using Mdaresna.Doamin.DTOs.ReportingDTOs.StudentWeeklyReportDTOs;
using Mdaresna.Doamin.Enums;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Repository.IServices;
using Microsoft.EntityFrameworkCore;

namespace Mdarens.ReportingWorker;

public class ReportingJob : IReportingJob
{
    private readonly ILogger<ReportingJob> _logger;
    private readonly IReportProcessingService _reportProcessingService;

    public ReportingJob(
        ILogger<ReportingJob> logger,
        IReportProcessingService reportProcessingService)
    {
        _logger = logger;
        _reportProcessingService = reportProcessingService;
    }

    [Queue("reports")]
    [DisableConcurrentExecution(timeoutInSeconds: 60 * 60)]
    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessPendingReportsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reporting job started at {Time}", DateTimeOffset.Now);

        await _reportProcessingService.ProcessPendingReportsAsync(cancellationToken);

        _logger.LogInformation("Reporting job finished at {Time}", DateTimeOffset.Now);
    }
}
