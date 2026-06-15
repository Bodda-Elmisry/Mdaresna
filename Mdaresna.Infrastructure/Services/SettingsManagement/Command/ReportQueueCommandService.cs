using Mdaresna.Doamin.DTOs.SettingsManagement;
using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.ReportingManagement;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SettingsManagement.Command;
using Mdaresna.Repository.MainDB.IFactories;
using Mdaresna.Repository.MainDB.IUnitOfWorks;
using Mdaresna.Repository.IServices.SettingsManagement.Command;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Mdaresna.Infrastructure.Services.SettingsManagement.Command
{
    public class ReportQueueCommandService : IReportQueueCommandService
    {
        private static readonly Guid ReportingServiceId = Guid.Parse("8488E63B-FD78-43BF-800B-03412C372DB5");

        private readonly IReportQueueCommandRepository reportQueueCommandRepository;
        private readonly IBaseSharedRepository<ReportQueue> sharedRepository;
        private readonly AppDbContext context;
        private readonly IMainUnitOfWork mainUnitOfWork;
        private readonly IDBFactory dbFactory;
        private readonly INotificationFactory notificationFactory;

        public ReportQueueCommandService(
            IReportQueueCommandRepository reportQueueCommandRepository,
            IBaseSharedRepository<ReportQueue> sharedRepository,
            AppDbContext context,
            IMainUnitOfWork mainUnitOfWork,
            IDBFactory dbFactory,
            INotificationFactory notificationFactory)
        {
            this.reportQueueCommandRepository = reportQueueCommandRepository;
            this.sharedRepository = sharedRepository;
            this.context = context;
            this.mainUnitOfWork = mainUnitOfWork;
            this.dbFactory = dbFactory;
            this.notificationFactory = notificationFactory;
        }

        public bool Create(ReportQueue entity)
        {
            entity.Id = DataGenerationHelper.GenerateRowId();
            entity.CreatedAt = DateTime.UtcNow;
            entity.Status = ReportQueueStatusEnum.Queued;
            entity.RetryCount = 0;
            entity.StartedAt = null;
            entity.CompletedAt = null;
            entity.Errors = null;

            return reportQueueCommandRepository.Create(entity);
        }

        public async Task<bool> DeleteAsync(ReportQueue entity)
        {
            var reportQueue = await sharedRepository.GetAsync(entity.Id);

            if (reportQueue == null)
            {
                return false;
            }

            return reportQueueCommandRepository.Delete(reportQueue);
        }

        public bool Update(ReportQueue entity)
        {
            return reportQueueCommandRepository.Update(entity);
        }

        public async Task<bool> MarkStartedAsync(Guid id)
        {
            var reportQueue = await sharedRepository.GetAsync(id);

            if (reportQueue == null)
            {
                return false;
            }

            reportQueue.Status = ReportQueueStatusEnum.Processing;
            reportQueue.StartedAt ??= DateTime.UtcNow;
            reportQueue.CompletedAt = null;

            return reportQueueCommandRepository.Update(reportQueue);
        }

        public async Task<bool> MarkCompletedAsync(Guid id, int? affectedRows = null, string? notes = null)
        {
            var reportQueue = await sharedRepository.GetAsync(id);

            if (reportQueue == null)
            {
                return false;
            }

            reportQueue.Status = ReportQueueStatusEnum.PendingReview;
            reportQueue.StartedAt ??= DateTime.UtcNow;
            reportQueue.CompletedAt = DateTime.UtcNow;
            reportQueue.Errors = null;
            reportQueue.AffectedRows = affectedRows;
            reportQueue.Notes = notes;

            return reportQueueCommandRepository.Update(reportQueue);
        }

        public async Task<bool> MarkFailedAsync(
            Guid id,
            string errors,
            int? affectedRows = null,
            string? notes = null)
        {
            var reportQueue = await sharedRepository.GetAsync(id);

            if (reportQueue == null)
            {
                return false;
            }

            reportQueue.Status = ReportQueueStatusEnum.Failed;
            reportQueue.StartedAt ??= DateTime.UtcNow;
            reportQueue.CompletedAt = DateTime.UtcNow;
            reportQueue.Errors = errors;
            reportQueue.AffectedRows = affectedRows;
            reportQueue.RetryCount += 1;
            reportQueue.Notes = notes;

            return reportQueueCommandRepository.Update(reportQueue);
        }

        private async Task<(User requestedBy, School school, SchoolYearMonth month, SchoolGrade? grade, ClassRoom? classroom)> ValidateRequestMonthReportAsync(RequestMonthReportCommandDTO command)
        {
            ArgumentNullException.ThrowIfNull(command);

            var requestedById = command.GetRequestedById();
            if (requestedById == Guid.Empty)
            {
                throw new ArgumentException("Created by id is required.", nameof(command));
            }

            if (command.SchoolId == Guid.Empty)
            {
                throw new ArgumentException("School id is required.", nameof(command));
            }

            if (command.MonthId == Guid.Empty)
            {
                throw new ArgumentException("Month id is required.", nameof(command));
            }

            if (command.ToDate.Date < command.FromDate.Date)
            {
                throw new ArgumentException("To date must be on or after from date.", nameof(command));
            }

            var school = await context.Schools
                .FirstOrDefaultAsync(item => item.Id == command.SchoolId && item.Deleted == false)
                ?? throw new InvalidOperationException("School was not found.");

            var month = await context.SchoolYearMonths
                .Include(item => item.Year)
                .FirstOrDefaultAsync(item => item.Id == command.MonthId && item.Deleted == false)
                ?? throw new InvalidOperationException("Month was not found.");

            if(!command.GradeId.HasValue && !command.ClassroomId.HasValue)
            {
                var isMonthGrageReported = await context.ReportQueues.AnyAsync(e => e.SchoolId == command.SchoolId && e.MonthId == command.MonthId && e.Status != ReportQueueStatusEnum.Failed && e.Status != ReportQueueStatusEnum.Published);
                if (isMonthGrageReported)
                    throw new InvalidOperationException("Report generated full or partial for this month.");
            }

            if (month.Year.SchoolId != command.SchoolId)
            {
                throw new InvalidOperationException("Month does not belong to the selected school.");
            }

            var requestedBy = await context.Users
                .FirstOrDefaultAsync(item => item.Id == requestedById && item.Deleted == false)
                ?? throw new InvalidOperationException("Requested by user was not found.");

            SchoolGrade? grade = null;

            if (command.GradeId.HasValue)
            {
                grade = await context.SchoolGrades.FirstOrDefaultAsync(item =>
                    item.Id == command.GradeId.Value &&
                    item.SchoolId == command.SchoolId &&
                    item.Deleted == false);

                if (grade is null)
                {
                    throw new InvalidOperationException("Grade was not found.");
                }
                else if(!command.ClassroomId.HasValue)
                {
                    var isMonthGrageReported = await context.ReportQueues.AnyAsync(e => e.SchoolId == command.SchoolId && e.MonthId == command.MonthId && e.GradeId == command.GradeId && e.Status != ReportQueueStatusEnum.Failed && e.Status != ReportQueueStatusEnum.Published);
                    if(isMonthGrageReported)
                        throw new InvalidOperationException("Report generated full or partial for this grade.");
                }
            }


            ClassRoom? classroom = null;
            
            if(command.ClassroomId.HasValue)
            {
                classroom = await context.ClassRooms.FirstOrDefaultAsync(item =>
                    item.Id == command.ClassroomId.Value &&
                    item.SchoolId == command.SchoolId &&
                    item.Deleted == false);

                if (classroom is null)
                {
                    throw new InvalidOperationException("Classroom was not found.");
                }

                else
                {
                    var isMonthGrageReported = await context.ReportQueues.AnyAsync(e => e.SchoolId == command.SchoolId && e.MonthId == command.MonthId && e.GradeId == command.GradeId && e.ClassroomId == command.ClassroomId && e.Status != ReportQueueStatusEnum.Failed && e.Status != ReportQueueStatusEnum.Published);
                    if (isMonthGrageReported)
                        throw new InvalidOperationException("Report generated full or partial for this classroom.");
                }
            }

            return (requestedBy, school, month, grade, classroom);
        }

        public async Task<RequestMonthReportResponseDTO> RequestMonthReportAsync(RequestMonthReportCommandDTO command)
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var validateionResponse = await ValidateRequestMonthReportAsync(command);
                var requestedAt = DateTime.UtcNow;
                var reportQueue = new ReportQueue
                {
                    Id = DataGenerationHelper.GenerateRowId(),
                    SchoolId = command.SchoolId,
                    GradeId = command.GradeId,
                    ClassroomId = command.ClassroomId,
                    FromDate = command.FromDate.Date,
                    ToDate = command.ToDate.Date,
                    MonthId = command.MonthId,
                    ReportType = StudentReportTypesEnum.Monthly,
                    Status = ReportQueueStatusEnum.Queued,
                    CreatedById = validateionResponse.requestedBy.Id,
                    CreatedAt = requestedAt,
                    RetryCount = 0,
                    StartedAt = null,
                    CompletedAt = null,
                    Errors = null
                };

                

                context.ReportQueues.Add(reportQueue);


                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new RequestMonthReportResponseDTO
                {
                    SchoolId = validateionResponse.school.Id,
                    SchoolName = validateionResponse.school.Name,
                    GradeId = validateionResponse.grade?.Id,
                    GradeName = validateionResponse.grade?.Name,
                    ClassroomId = validateionResponse.classroom?.Id,
                    ClassroomName = validateionResponse.classroom?.Name,
                    MonthId = validateionResponse.month.Id,
                    MonthName = validateionResponse.month.Name,
                    FromDate = reportQueue.FromDate,
                    ToDate = reportQueue.ToDate,
                    RequestedById = validateionResponse.requestedBy.Id,
                    RequestedByName = GetUserFullName(validateionResponse.requestedBy.FirstName, validateionResponse.requestedBy.LastName),
                    RequestedAt = reportQueue.CreatedAt
                };
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                throw ex;
            }
            
        }

        private static string GetUserFullName(string firstName, string lastName)
        {
            return $"{firstName} {lastName}".Trim();
        }

        public async Task<PublishReportQueueResponseDTO> PublishReportQueueAsync(
            PublishReportQueueCommandDTO command,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

            if (command.ReportQueueId == Guid.Empty)
            {
                throw new ArgumentException("Report queue id is required.", nameof(command));
            }

            var queue = await context.ReportQueues
                .Include(item => item.Month)
                .FirstOrDefaultAsync(item => item.Id == command.ReportQueueId, cancellationToken)
                ?? throw new InvalidOperationException("Report queue was not found.");

            if (queue.Status == ReportQueueStatusEnum.Published)
            {
                throw new InvalidOperationException("Report queue is already published.");
            }

            if (queue.Status != ReportQueueStatusEnum.PendingReview)
            {
                throw new InvalidOperationException("Only pending review report queues can be published.");
            }

            var publisherId = command.GetPublisherId();
            if (publisherId.HasValue)
            {
                var publisherExists = await context.Users
                    .AsNoTracking()
                    .AnyAsync(user => user.Id == publisherId.Value && user.Deleted == false, cancellationToken);

                if (!publisherExists)
                {
                    throw new InvalidOperationException("Publisher user was not found.");
                }
            }

            var reportingConnectionString = await GetReportingConnectionStringAsync(
                queue.SchoolId,
                cancellationToken);
            var activationResult = await ActivatePublishedStudentReportsAsync(
                queue,
                reportingConnectionString,
                cancellationToken);

            if (activationResult.StudentIds.Count == 0)
            {
                throw new InvalidOperationException("No generated student reports were found for this queue.");
            }

            var targets = await GetStudentParentNotificationTargetsAsync(
                queue,
                activationResult.StudentIds,
                cancellationToken);

            var publishedAt = DateTime.UtcNow;
            queue.Status = ReportQueueStatusEnum.Published;
            queue.PublishedAt = publishedAt;
            queue.ReviewdById = publisherId;
            queue.Errors = null;

            await context.SaveChangesAsync(cancellationToken);

            var notificationResult = await SendPublishNotificationsAsync(
                targets,
                GetMonthName(queue),
                cancellationToken);

            return new PublishReportQueueResponseDTO
            {
                ReportQueueId = queue.Id,
                SchoolId = queue.SchoolId,
                MonthId = queue.MonthId,
                MonthName = GetMonthName(queue),
                Status = queue.Status,
                PublishedAt = publishedAt,
                PublishedById = publisherId,
                ReportsCount = activationResult.ReportsCount,
                StudentsCount = activationResult.StudentIds.Count,
                ParentsCount = targets.Select(item => item.ParentId).Distinct().Count(),
                NotificationsAttempted = notificationResult.Attempted,
                NotificationsFailed = notificationResult.Failed
            };
        }

        public async Task<FailReportQueueResponseDTO> FailReportQueueAsync(
            FailReportQueueCommandDTO command,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

            if (command.ReportQueueId == Guid.Empty)
            {
                throw new ArgumentException("Report queue id is required.", nameof(command));
            }

            var reason = command.Reason.Trim();
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("Fail reason is required.", nameof(command));
            }

            var queue = await context.ReportQueues
                .FirstOrDefaultAsync(item => item.Id == command.ReportQueueId, cancellationToken)
                ?? throw new InvalidOperationException("Report queue was not found.");

            if (queue.Status == ReportQueueStatusEnum.Published)
            {
                throw new InvalidOperationException("Published report queues cannot be failed.");
            }

            if (queue.Status != ReportQueueStatusEnum.PendingReview &&
                queue.Status != ReportQueueStatusEnum.Failed)
            {
                throw new InvalidOperationException("Only pending review or failed report queues can be failed.");
            }

            var reviewerId = command.GetReviewerId();
            if (reviewerId.HasValue)
            {
                var reviewerExists = await context.Users
                    .AsNoTracking()
                    .AnyAsync(user => user.Id == reviewerId.Value && user.Deleted == false, cancellationToken);

                if (!reviewerExists)
                {
                    throw new InvalidOperationException("Reviewer user was not found.");
                }
            }

            var failedAt = DateTime.UtcNow;
            queue.Status = ReportQueueStatusEnum.Failed;
            queue.CompletedAt = failedAt;
            queue.ReviewdById = reviewerId;
            queue.Notes = AppendQueueNote(queue.Notes, reason);

            await context.SaveChangesAsync(cancellationToken);

            return new FailReportQueueResponseDTO
            {
                ReportQueueId = queue.Id,
                SchoolId = queue.SchoolId,
                MonthId = queue.MonthId,
                Status = queue.Status,
                FailedById = reviewerId,
                FailedAt = failedAt,
                Notes = queue.Notes ?? string.Empty
            };
        }

        private async Task<string> GetReportingConnectionStringAsync(
            Guid schoolId,
            CancellationToken cancellationToken)
        {
            var reportingService = await mainUnitOfWork.MdaresnaSchoolService
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    service => service.SchoolId == schoolId && service.ServiceId == ReportingServiceId,
                    cancellationToken)
                ?? throw new InvalidOperationException("Reporting service is not configured for this school.");

            return dbFactory.GetConnectionString(
                reportingService.DBType,
                reportingService.DBSource,
                reportingService.DBUser,
                reportingService.DBPassword,
                reportingService.DBCatlog,
                reportingService.DBPort);
        }

        private async Task<PublishedStudentReportActivationResult> ActivatePublishedStudentReportsAsync(
            ReportQueue queue,
            string reportingConnectionString,
            CancellationToken cancellationToken)
        {
            if (!queue.StartedAt.HasValue || !queue.CompletedAt.HasValue)
            {
                throw new InvalidOperationException("Report queue generation timestamps are required before publishing.");
            }

            var scopedStudentIds = await GetQueueScopeStudentIdsAsync(queue, cancellationToken);
            if (scopedStudentIds.Count == 0)
            {
                return PublishedStudentReportActivationResult.Empty;
            }

            await using var reportContext = SchoolReportDBContext.Create(reportingConnectionString);

            var monthReports = await ApplyReportQueueIdentityFilter(
                    reportContext.StudentReports,
                    queue)
                .ToListAsync(cancellationToken);

            if (monthReports.Count == 0)
            {
                return PublishedStudentReportActivationResult.Empty;
            }

            var scopedStudentIdSet = scopedStudentIds.ToHashSet();
            var scopedReports = monthReports
                .Where(report => scopedStudentIdSet.Contains(report.StudentId))
                .ToList();

            var currentReports = scopedReports
                .Where(report => report.ReportQueueId == queue.Id)
                .ToList();

            if (currentReports.Count == 0)
            {
                currentReports = scopedReports
                    .Where(report =>
                        !report.ReportQueueId.HasValue &&
                        report.CreatedAt >= queue.StartedAt.Value &&
                        report.CreatedAt <= queue.CompletedAt.Value)
                    .ToList();
            }

            if (currentReports.Count == 0)
            {
                return PublishedStudentReportActivationResult.Empty;
            }

            foreach (var report in scopedReports)
            {
                report.IsActive = false;
            }

            foreach (var report in currentReports)
            {
                report.IsActive = true;
            }

            await reportContext.SaveChangesAsync(cancellationToken);

            return new PublishedStudentReportActivationResult
            {
                StudentIds = currentReports
                    .Select(report => report.StudentId)
                    .Distinct()
                    .ToList(),
                ReportsCount = currentReports.Count
            };
        }

        private async Task<IReadOnlyList<Guid>> GetQueueScopeStudentIdsAsync(
            ReportQueue queue,
            CancellationToken cancellationToken)
        {
            var studentsQuery = context.Students
                .AsNoTracking()
                .Where(student =>
                    student.SchoolId == queue.SchoolId &&
                    student.Deleted == false);

            if (queue.ClassroomId.HasValue)
            {
                studentsQuery = studentsQuery.Where(student => student.ClassRoomId == queue.ClassroomId.Value);
            }
            else if (queue.GradeId.HasValue)
            {
                studentsQuery = studentsQuery.Where(student =>
                    context.ClassRooms.Any(classRoom =>
                        classRoom.Id == student.ClassRoomId &&
                        classRoom.GradeId == queue.GradeId.Value &&
                        classRoom.Deleted == false));
            }

            return await studentsQuery
                .Select(student => student.Id)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private static IQueryable<StudentReport> ApplyReportQueueIdentityFilter(
            IQueryable<StudentReport> reportsQuery,
            ReportQueue queue)
        {
            reportsQuery = reportsQuery.Where(report =>
                report.SchoolId == queue.SchoolId &&
                report.ReportType == queue.ReportType);

            reportsQuery = queue.MonthId.HasValue
                ? reportsQuery.Where(report => report.MonthId == queue.MonthId.Value)
                : reportsQuery.Where(report => report.MonthId == null);

            reportsQuery = string.IsNullOrWhiteSpace(queue.WeekName)
                ? reportsQuery.Where(report => report.WeekName == null || report.WeekName == string.Empty)
                : reportsQuery.Where(report => report.WeekName == queue.WeekName);

            return reportsQuery;
        }

        private async Task<IReadOnlyList<StudentParentNotificationTarget>> GetStudentParentNotificationTargetsAsync(
            ReportQueue queue,
            IReadOnlyCollection<Guid> studentIds,
            CancellationToken cancellationToken)
        {
            if (studentIds.Count == 0)
            {
                return Array.Empty<StudentParentNotificationTarget>();
            }

            var targets = await (
                from student in context.Students.AsNoTracking()
                join studentParent in context.StudentParents.AsNoTracking()
                    on student.Id equals studentParent.StudentId
                join userDevice in context.UserDevices.AsNoTracking()
                    on studentParent.ParentId equals userDevice.UserId
                where
                    studentIds.Contains(student.Id) &&
                    student.SchoolId == queue.SchoolId &&
                    student.Deleted == false &&
                    studentParent.Deleted == false &&
                    userDevice.Deleted == false &&
                    userDevice.FcmToken != null &&
                    userDevice.FcmToken != string.Empty
                select new StudentParentNotificationTarget
                {
                    StudentId = student.Id,
                    StudentName = (student.FirstName + " " + student.MiddelName + " " + student.LastName).Trim(),
                    ParentId = studentParent.ParentId,
                    FcmToken = userDevice.FcmToken
                })
                .ToListAsync(cancellationToken);

            return targets
                .GroupBy(item => new { item.StudentId, item.ParentId, item.FcmToken })
                .Select(group => group.First())
                .ToList();
        }

        private async Task<(int Attempted, int Failed)> SendPublishNotificationsAsync(
            IReadOnlyCollection<StudentParentNotificationTarget> targets,
            string monthName,
            CancellationToken cancellationToken)
        {
            if (targets.Count == 0)
            {
                return (0, 0);
            }

            var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
            var attempted = 0;
            var failed = 0;

            foreach (var studentGroup in targets.GroupBy(item => new { item.StudentId, item.StudentName }))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var tokens = studentGroup
                    .Select(item => item.FcmToken)
                    .Where(token => !string.IsNullOrWhiteSpace(token))
                    .Distinct()
                    .ToList();

                if (tokens.Count == 0)
                {
                    continue;
                }

                var body = $"تم انشاء تقرير شهر {monthName} لابنك {studentGroup.Key.StudentName}";
                attempted += tokens.Count;

                try
                {
                    await notificationProvider.SendToMultiUsersAsync(tokens, "تقرير شهري", body);
                }
                catch
                {
                    failed += tokens.Count;
                }
            }

            return (attempted, failed);
        }

        private static string GetMonthName(ReportQueue queue)
        {
            if (!string.IsNullOrWhiteSpace(queue.Month?.Name))
            {
                return queue.Month.Name;
            }

            return queue.FromDate.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("ar-EG"));
        }

        private static string AppendQueueNote(string? existingNotes, string reason)
        {
            var normalizedReason = reason.Trim();
            if (string.IsNullOrWhiteSpace(existingNotes))
            {
                return normalizedReason;
            }

            return $"{existingNotes.TrimEnd()}{Environment.NewLine}{normalizedReason}";
        }

        private sealed class StudentParentNotificationTarget
        {
            public Guid StudentId { get; set; }

            public string StudentName { get; set; } = string.Empty;

            public Guid ParentId { get; set; }

            public string FcmToken { get; set; } = string.Empty;
        }

        private sealed class PublishedStudentReportActivationResult
        {
            public static PublishedStudentReportActivationResult Empty { get; } = new()
            {
                StudentIds = Array.Empty<Guid>(),
                ReportsCount = 0
            };

            public IReadOnlyList<Guid> StudentIds { get; set; } = Array.Empty<Guid>();

            public int ReportsCount { get; set; }
        }
    }
}
