using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.DTOs.ReportingDTOs;
using Mdaresna.Doamin.Models.ReportingManagement;
using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SettingsManagement.Query;
using Mdaresna.Repository.MainDB.IFactories;
using Mdaresna.Repository.MainDB.IUnitOfWorks;
using Mdaresna.Repository.IServices.SettingsManagement.Query;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Infrastructure.Services.SettingsManagement.Query
{
    public class ReportQueueQueryService : IReportQueueQueryService
    {
        private static readonly Guid ReportingServiceId = Guid.Parse("8488E63B-FD78-43BF-800B-03412C372DB5");

        private readonly IReportQueueQueryRepository reportQueueQueryRepository;
        private readonly IBaseSharedRepository<ReportQueue> sharedRepository;
        private readonly IMainUnitOfWork mainUnitOfWork;
        private readonly IDBFactory dbFactory;

        public ReportQueueQueryService(
            IReportQueueQueryRepository reportQueueQueryRepository,
            IBaseSharedRepository<ReportQueue> sharedRepository,
            IMainUnitOfWork mainUnitOfWork,
            IDBFactory dbFactory)
        {
            this.reportQueueQueryRepository = reportQueueQueryRepository;
            this.sharedRepository = sharedRepository;
            this.mainUnitOfWork = mainUnitOfWork;
            this.dbFactory = dbFactory;
        }

        public async Task<IEnumerable<ReportQueue>> GetAllAsync()
        {
            return await reportQueueQueryRepository.GetAllAsync();
        }

        public async Task<ReportQueue> GetByIdAsync(Guid id)
        {
            return await sharedRepository.GetAsync(id);
        }

        public async Task<ReportQueue?> GetByIdWithDetailsAsync(Guid id)
        {
            return await reportQueueQueryRepository.GetByIdWithDetailsAsync(id);
        }

        public async Task<IEnumerable<ReportQueue>> GetPendingAsync(int take)
        {
            return await reportQueueQueryRepository.GetPendingAsync(take);
        }

        public async Task<IEnumerable<ReportQueue>> GetBySchoolAsync(
            Guid schoolId,
            Guid? monthId = null,
            Guid? gradeId = null,
            Guid? classroomId = null,
            ReportQueueStatusEnum? status = null,
            StudentReportTypesEnum? reportType = null)
        {
            return await reportQueueQueryRepository.GetBySchoolAsync(
                schoolId,
                monthId,
                gradeId,
                classroomId,
                status,
                reportType);
        }

        public async Task<PagedResultDTO<ReportQueue>> GetBySchoolPagedAsync(
            Guid schoolId,
            Guid? monthId = null,
            Guid? gradeId = null,
            Guid? classroomId = null,
            ReportQueueStatusEnum? status = null,
            StudentReportTypesEnum? reportType = null,
            int pageNumber = 1,
            int? pageSize = null)
        {
            return await reportQueueQueryRepository.GetBySchoolPagedAsync(
                schoolId,
                monthId,
                gradeId,
                classroomId,
                status,
                reportType,
                pageNumber,
                pageSize);
        }

        public async Task<IReadOnlyList<StudentReportResultDTO>> GetStudentReportsByReportIdAsync(
            Guid reportQueueId,
            Guid? gradeId = null,
            Guid? classroomId = null,
            CancellationToken cancellationToken = default)
        {
            if (reportQueueId == Guid.Empty)
            {
                throw new ArgumentException("Report id is required.", nameof(reportQueueId));
            }

            var queue = await reportQueueQueryRepository.GetByIdWithDetailsAsync(reportQueueId)
                ?? throw new InvalidOperationException("Report queue was not found.");

            var reportingConnectionString = await GetReportingConnectionStringAsync(
                queue.SchoolId,
                cancellationToken);

            await using var reportContext = await SchoolReportDBContext.CreateAndMigrateAsync(
                reportingConnectionString,
                cancellationToken);

            var query = reportContext.StudentReports
                .AsNoTracking()
                .Where(report => report.ReportQueueId == reportQueueId);

            if (classroomId.HasValue && classroomId.Value != Guid.Empty)
            {
                query = query.Where(report => report.ClassRoomId == classroomId.Value);
            }
            else if (gradeId.HasValue && gradeId.Value != Guid.Empty)
            {
                query = query.Where(report => report.GradeId == gradeId.Value);
            }

            var reports = await ReadStudentReportsAsync(query, cancellationToken);

            if (reports.Count > 0 || !queue.StartedAt.HasValue || !queue.CompletedAt.HasValue)
            {
                return reports;
            }

            var fallbackQuery = ApplyReportQueueFallbackFilter(
                reportContext.StudentReports.AsNoTracking(),
                queue);

            if (classroomId.HasValue && classroomId.Value != Guid.Empty)
            {
                fallbackQuery = fallbackQuery.Where(report => report.ClassRoomId == classroomId.Value);
            }
            else if (gradeId.HasValue && gradeId.Value != Guid.Empty)
            {
                fallbackQuery = fallbackQuery.Where(report => report.GradeId == gradeId.Value);
            }

            return await ReadStudentReportsAsync(fallbackQuery, cancellationToken);
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

        private static IQueryable<StudentReport> ApplyReportQueueFallbackFilter(
            IQueryable<StudentReport> reportsQuery,
            ReportQueue queue)
        {
            reportsQuery = reportsQuery.Where(report =>
                !report.ReportQueueId.HasValue &&
                report.SchoolId == queue.SchoolId &&
                report.ReportType == queue.ReportType &&
                report.CreatedAt >= queue.StartedAt!.Value &&
                report.CreatedAt <= queue.CompletedAt!.Value);

            reportsQuery = queue.MonthId.HasValue
                ? reportsQuery.Where(report => report.MonthId == queue.MonthId.Value)
                : reportsQuery.Where(report => report.MonthId == null);

            reportsQuery = string.IsNullOrWhiteSpace(queue.WeekName)
                ? reportsQuery.Where(report => report.WeekName == null || report.WeekName == string.Empty)
                : reportsQuery.Where(report => report.WeekName == queue.WeekName);

            return reportsQuery;
        }

        private static async Task<IReadOnlyList<StudentReportResultDTO>> ReadStudentReportsAsync(
            IQueryable<StudentReport> reportsQuery,
            CancellationToken cancellationToken)
        {
            return await reportsQuery
                .OrderBy(report => report.GradeId)
                .ThenBy(report => report.ClassRoomId)
                .ThenBy(report => report.StudentId)
                .ThenBy(report => report.Id)
                .Select(report => new StudentReportResultDTO
                {
                    Id = report.Id,
                    SchoolId = report.SchoolId,
                    StudentId = report.StudentId,
                    ReportQueueId = report.ReportQueueId,
                    GradeId = report.GradeId,
                    ClassRoomId = report.ClassRoomId,
                    MonthId = report.MonthId,
                    WeekName = report.WeekName,
                    ReportDetails = report.ReportDetails,
                    CreatedAt = report.CreatedAt,
                    IsActive = report.IsActive,
                    Version = report.Version,
                    ReportType = report.ReportType
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ReportQueue>> GetStudentReportsQueuesAsync(
            Guid schoolId,
            Guid studentId,
            CancellationToken cancellationToken = default)
        {
            var reportingConnectionString = await GetReportingConnectionStringAsync(
                schoolId,
                cancellationToken);

            await using var reportContext = await SchoolReportDBContext.CreateAndMigrateAsync(
                reportingConnectionString,
                cancellationToken);

            var reportQueueIds = await reportContext.StudentReports
                .AsNoTracking()
                .Where(r => r.StudentId == studentId && r.ReportQueueId != null)
                .Select(r => r.ReportQueueId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (reportQueueIds.Count == 0)
            {
                return new List<ReportQueue>();
            }

            var queues = await reportQueueQueryRepository.GetByIdsWithDetailsAsync(reportQueueIds);
            return queues.ToList();
        }
    }
}
