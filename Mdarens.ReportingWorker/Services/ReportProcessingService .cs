using Mdarens.ReportingWorker.Factories;
using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.MainDB.IFactories;
using Mdaresna.Repository.MainDB.IServices;
using Mdaresna.Repository.MainDB.IUnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace Mdarens.ReportingWorker.Services;

public class ReportProcessingService : IReportProcessingService
{
    private static readonly Guid ReportingServiceId = Guid.Parse("8488E63B-FD78-43BF-800B-03412C372DB5");
    private const string ReviewMonthReportPermissionKey = "ReviewSchoolMonthReport";
    private const string ReviewNotificationTitleText = "تقرير شهري";

    private readonly IWorkerJobSettingsService settingsService;
    private readonly IMdaresnaSchoolService schoolConnectionService;
    private readonly ISchoolDbContextFactory schoolDbContextFactory;
    private readonly IStudentReportFactory studentReportFactory;
    private readonly IDBFactory dbFactory;
    private readonly IMainUnitOfWork mainUnitOfWork;
    private readonly INotificationFactory notificationFactory;
    private readonly ILogger<ReportProcessingService> logger;

    public ReportProcessingService(
        IWorkerJobSettingsService settingsService,
        IMdaresnaSchoolService schoolConnectionService,
        ISchoolDbContextFactory schoolDbContextFactory,
        IStudentReportFactory studentReportFactory,
        IDBFactory dbFactory,
        IMainUnitOfWork mainUnitOfWork,
        INotificationFactory notificationFactory,
        ILogger<ReportProcessingService> logger)
    {
        this.settingsService = settingsService;
        this.schoolConnectionService = schoolConnectionService;
        this.schoolDbContextFactory = schoolDbContextFactory;
        this.studentReportFactory = studentReportFactory;
        this.dbFactory = dbFactory;
        this.mainUnitOfWork = mainUnitOfWork;
        this.notificationFactory = notificationFactory;
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
                var reportGenerated = false;

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
                    reportGenerated = true;
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

                if (reportGenerated)
                {
                    await SendReviewNotificationAsync(dbContext, item, cancellationToken);
                }

                remainingItems--;
            }
        }

        await settingsService.MarkRunCompletedAsync("ProcessPendingReports", cancellationToken);
    }

    private async Task SendReviewNotificationAsync(
        AppDbContext dbContext,
        ReportQueue reportQueue,
        CancellationToken cancellationToken)
    {
        try
        {
            var tokens = await GetReviewMonthReportEmployeeTokensAsync(
                dbContext,
                reportQueue.SchoolId,
                cancellationToken);

            if (tokens.Count == 0)
            {
                logger.LogInformation(
                    "No user devices found for review month report notification. ReportQueueId: {ReportQueueId}, SchoolId: {SchoolId}, PermissionKey: {PermissionKey}",
                    reportQueue.Id,
                    reportQueue.SchoolId,
                    ReviewMonthReportPermissionKey);
                return;
            }

            var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
            var message = BuildCleanReviewNotificationMessage(reportQueue);

            await notificationProvider.SendToMultiUsersAsync(tokens, ReviewNotificationTitleText, message);

            logger.LogInformation(
                "Sent review month report notification for report queue {ReportQueueId} to {TokenCount} device(s).",
                reportQueue.Id,
                tokens.Count);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to send review month report notification for report queue {ReportQueueId}.",
                reportQueue.Id);
        }
    }

    private async Task<List<string>> GetReviewMonthReportEmployeeTokensAsync(
        AppDbContext dbContext,
        Guid schoolId,
        CancellationToken cancellationToken)
    {
        var permissionId = await dbContext.Permissions
            .AsNoTracking()
            .Where(permission =>
                permission.Key == ReviewMonthReportPermissionKey &&
                permission.Deleted == false)
            .Select(permission => permission.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (permissionId == Guid.Empty)
        {
            logger.LogWarning(
                "Permission {PermissionKey} was not found. Review month report notifications will not be sent.",
                ReviewMonthReportPermissionKey);
            return new List<string>();
        }

        var usersFromRoleQuery =
            from userRole in dbContext.UserRoles.AsNoTracking()
            join rolePermission in dbContext.RolePermissions.AsNoTracking()
                on userRole.RoleId equals rolePermission.RoleId
            where
                userRole.SchoolId == schoolId &&
                userRole.Deleted == false &&
                rolePermission.PermissionId == permissionId &&
                rolePermission.Deleted == false
            select userRole.UserId;

        var usersFromUserPermissionQuery =
            from userPermission in dbContext.userPermissions.AsNoTracking()
            where
                userPermission.SchoolId == schoolId &&
                userPermission.PermissionId == permissionId &&
                userPermission.Deleted == false
            select userPermission.UserId;

        var reviewerUserIds = usersFromRoleQuery
            .Union(usersFromUserPermissionQuery)
            .Distinct();

        var tokens = await (
            from userDevice in dbContext.UserDevices.AsNoTracking()
            where
                reviewerUserIds.Contains(userDevice.UserId) &&
                userDevice.Deleted == false &&
                userDevice.FcmToken != null &&
                userDevice.FcmToken != string.Empty
            select userDevice.FcmToken)
            .Distinct()
            .ToListAsync(cancellationToken);

        return tokens;
    }

    private static string BuildCleanReviewNotificationMessage(ReportQueue reportQueue)
    {
        var monthName = !string.IsNullOrWhiteSpace(reportQueue.Month?.Name)
            ? reportQueue.Month.Name
            : reportQueue.FromDate.ToString("MMMM yyyy");

        var message = $"تم انشاء تقارير شهر {monthName} للمدرسه {reportQueue.School.Name}";

        if (!string.IsNullOrWhiteSpace(reportQueue.Grade?.Name))
        {
            message += $" للمرحلة الدراسه {reportQueue.Grade.Name}";
        }

        if (!string.IsNullOrWhiteSpace(reportQueue.Classroom?.Name))
        {
            message += $" للفصل {reportQueue.Classroom.Name}";
        }

        return $"{message}|Type=ReportQueue|TargetId={reportQueue.Id}|SchoolId={reportQueue.SchoolId}";
    }

}
