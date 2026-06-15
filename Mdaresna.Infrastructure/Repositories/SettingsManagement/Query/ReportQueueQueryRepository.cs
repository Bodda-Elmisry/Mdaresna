using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Repository.IRepositories.SettingsManagement.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Mdaresna.Infrastructure.Repositories.SettingsManagement.Query
{
    public class ReportQueueQueryRepository : IReportQueueQueryRepository
    {
        private readonly AppDbContext context;
        private readonly AppSettingDTO appSettings;

        public ReportQueueQueryRepository(
            AppDbContext context,
            IOptions<AppSettingDTO> appSettings)
        {
            this.context = context;
            this.appSettings = appSettings.Value;
        }

        public IQueryable<ReportQueue> GetQuery()
        {
            return context.ReportQueues.AsQueryable();
        }

        public async Task<IEnumerable<ReportQueue>> GetAllAsync()
        {
            return await GetQuery()
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<ReportQueue> GetByIdAsync(Guid id)
        {
            return await context.ReportQueues.FindAsync(id) ?? null!;
        }

        public async Task<ReportQueue?> GetByIdWithDetailsAsync(Guid id)
        {
            return await GetQueueWithDetails()
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<IEnumerable<ReportQueue>> GetPendingAsync(int take)
        {
            var resolvedTake = take > 0 ? take : 30;

            return await GetQueueWithDetails()
                .Where(q => q.Status == ReportQueueStatusEnum.Queued || q.Status == ReportQueueStatusEnum.Failed)
                .OrderBy(q => q.CreatedAt)
                .Take(resolvedTake)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReportQueue>> GetBySchoolAsync(
            Guid schoolId,
            Guid? monthId = null,
            Guid? gradeId = null,
            Guid? classroomId = null,
            ReportQueueStatusEnum? status = null,
            StudentReportTypesEnum? reportType = null)
        {
            var query = ApplySchoolFilters(
                GetQueueWithDetails(),
                schoolId,
                monthId,
                gradeId,
                classroomId,
                status,
                reportType);

            return await query
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
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
            var resolvedPageNumber = pageNumber > 0 ? pageNumber : 1;
            var resolvedPageSize = pageSize.GetValueOrDefault(appSettings.PageSize ?? 30);
            if (resolvedPageSize <= 0)
            {
                resolvedPageSize = appSettings.PageSize ?? 30;
            }

            var query = ApplySchoolFilters(
                GetQueueWithDetails(),
                schoolId,
                monthId,
                gradeId,
                classroomId,
                status,
                reportType);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(q => q.CreatedAt)
                .Skip((resolvedPageNumber - 1) * resolvedPageSize)
                .Take(resolvedPageSize)
                .ToListAsync();

            return new PagedResultDTO<ReportQueue>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = resolvedPageNumber,
                PageSize = resolvedPageSize,
                TotalPages = totalCount == 0
                    ? 0
                    : (int)Math.Ceiling(totalCount / (decimal)resolvedPageSize)
            };
        }

        private IQueryable<ReportQueue> ApplySchoolFilters(
            IQueryable<ReportQueue> query,
            Guid schoolId,
            Guid? monthId = null,
            Guid? gradeId = null,
            Guid? classroomId = null,
            ReportQueueStatusEnum? status = null,
            StudentReportTypesEnum? reportType = null)
        {
            query = query.Where(q => q.SchoolId == schoolId);

            if (monthId.HasValue)
                query = query.Where(q => q.MonthId == monthId.Value);

            if (gradeId.HasValue)
                query = query.Where(q => q.GradeId == gradeId.Value);

            if (classroomId.HasValue)
                query = query.Where(q => q.ClassroomId == classroomId.Value);

            if (status.HasValue)
                query = query.Where(q => q.Status == status.Value);

            if (reportType.HasValue)
                query = query.Where(q => q.ReportType == reportType.Value);

            return query;
        }

        private IQueryable<ReportQueue> GetQueueWithDetails()
        {
            return context.ReportQueues
                .AsNoTracking()
                .Include(q => q.School)
                .Include(q => q.Grade)
                .Include(q => q.Classroom)
                .Include(q => q.Month)
                .Include(q => q.ReviewdBy)
                .Include(q => q.CreatedBy);
        }
    }
}
