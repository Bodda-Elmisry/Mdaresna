using Mdarens.ReportingWorker.Factories;
using Mdaresna.Doamin.Enums;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.MainDB.IFactories;
using Mdaresna.Repository.MainDB.IServices;
using Mdaresna.Repository.MainDB.IUnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace Mdarens.ReportingWorker.Services;

public class ReportProcessingService : IReportProcessingService
{
    private static readonly Guid ReportingServiceId = Guid.Parse("8488E63B-FD78-43BF-800B-03412C372DB5");

    private readonly IWorkerJobSettingsService settingsService;
    private readonly IMdaresnaSchoolService schoolConnectionService;
    private readonly ISchoolDbContextFactory schoolDbContextFactory;
    private readonly IStudentReportFactory studentReportFactory;
    private readonly IDBFactory dbFactory;
    private readonly IMainUnitOfWork mainUnitOfWork;
    private readonly ILogger<ReportProcessingService> logger;

    public ReportProcessingService(
        IWorkerJobSettingsService settingsService,
        IMdaresnaSchoolService schoolConnectionService,
        ISchoolDbContextFactory schoolDbContextFactory,
        IStudentReportFactory studentReportFactory,
        IDBFactory dbFactory,
        IMainUnitOfWork mainUnitOfWork,
        ILogger<ReportProcessingService> logger)
    {
        this.settingsService = settingsService;
        this.schoolConnectionService = schoolConnectionService;
        this.schoolDbContextFactory = schoolDbContextFactory;
        this.studentReportFactory = studentReportFactory;
        this.dbFactory = dbFactory;
        this.mainUnitOfWork = mainUnitOfWork;
        this.logger = logger;
    }

    public async Task ProcessPendingReportsAsync(CancellationToken cancellationToken)
    {
        var settings = await settingsService.GetAsync("ProcessPendingReports", cancellationToken);

        if (settings is null || !settings.IsEnabled)
        {
            return;
        }

        if (!settings.ShouldRunNow(DateTime.UtcNow))
        {
            return;
        }

        var schools = await schoolConnectionService.GetActiveSchools();
        var remainingItems = settings.MaxItemsPerRun > 0 ? settings.MaxItemsPerRun : 500;

        foreach (var school in schools)
        {
            if (remainingItems <= 0)
            {
                break;
            }

            var schoolConnectionString = dbFactory.GetConnectionString(
                school.DBType,
                school.DBSource,
                school.DBUser,
                school.DBPassword,
                school.DBCatlog,
                school.DBPort);

            await using var dbContext = schoolDbContextFactory.CreateDbContext(schoolConnectionString);

            var reportingService = await mainUnitOfWork.MdaresnaSchoolService
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    service => service.SchoolId == school.Id && service.ServiceId == ReportingServiceId,
                    cancellationToken);

            if (reportingService is null)
            {
                logger.LogError("There is no reporting service configured for school {SchoolId} - {SchoolName}.", school.Id, school.Name);
                continue;
            }

            var reportingConnectionString = dbFactory.GetConnectionString(
                reportingService.DBType,
                reportingService.DBSource,
                reportingService.DBUser,
                reportingService.DBPassword,
                reportingService.DBCatlog,
                reportingService.DBPort);

            var take = Math.Min(settings.BatchSize > 0 ? settings.BatchSize : 30, remainingItems);
            var pendingItems = await dbContext.ReportQueues
                .Include(q => q.School)
                .Include(q => q.Grade)
                .Include(q => q.Classroom)
                .Include(q => q.Month)
                .Include(q => q.ReviewdBy)
                .Include(q => q.CreatedBy)
                .Where(q =>
                    q.SchoolId == school.Id &&
                    (q.Status == ReportQueueStatusEnum.Queued || q.Status == ReportQueueStatusEnum.Failed))
                .OrderBy(q => q.CreatedAt)
                .Take(take)
                .ToListAsync(cancellationToken);

            foreach (var item in pendingItems)
            {
                try
                {
                    item.Status = ReportQueueStatusEnum.Processing;
                    item.StartedAt = DateTime.UtcNow;
                    item.CompletedAt = null;
                    await dbContext.SaveChangesAsync(cancellationToken);

                    var affectedRows = await studentReportFactory.GenerateAsync(
                        item,
                        schoolConnectionString,
                        reportingConnectionString,
                        cancellationToken);

                    item.Status = ReportQueueStatusEnum.PendingReview;
                    item.CompletedAt = DateTime.UtcNow;
                    item.AffectedRows = affectedRows;
                    item.Errors = null;
                }
                catch (Exception ex)
                {
                    item.Status = ReportQueueStatusEnum.Failed;
                    item.CompletedAt = DateTime.UtcNow;
                    item.RetryCount += 1;
                    item.Errors = string.IsNullOrEmpty(item.Errors)
                        ? ex.Message
                        : $"{item.Errors} |&&| {ex.Message}";

                    logger.LogError(ex, "Failed to generate report item {ReportItemId}", item.Id);
                }

                await dbContext.SaveChangesAsync(cancellationToken);
                remainingItems--;
            }
        }

        await settingsService.MarkRunCompletedAsync("ProcessPendingReports", cancellationToken);
    }
}
