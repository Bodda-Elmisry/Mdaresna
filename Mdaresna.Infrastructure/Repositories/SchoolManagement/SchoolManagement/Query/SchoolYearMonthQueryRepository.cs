using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Query
{
    public class SchoolYearMonthQueryRepository : BaseQueryRepository<SchoolYearMonth>, ISchoolYearMonthQueryRepository
    {
        private readonly AppDbContext context;

        public SchoolYearMonthQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<SchoolYearMonthResultDTO>> GetYearMonthesAsync(Guid yearId, bool? isActive, string name)
        {
            var monthsQuery = context.SchoolYearMonths
                .AsNoTracking()
                .Where(m => m.YearId == yearId && m.Deleted == false);

            if (isActive.HasValue)
                monthsQuery = monthsQuery.Where(m => m.IsActive == isActive.Value);

            if(!string.IsNullOrEmpty(name))
                monthsQuery = monthsQuery.Where(m => m.Name.Contains(name));

            var months = await monthsQuery.ToListAsync();
            var reportQueuesByMonth = await GetLatestFullMonthReportQueuesAsync(
                months.Select(month => month.Id).ToList());

            return months.Select(month =>
            {
                reportQueuesByMonth.TryGetValue(month.Id, out var reportQueue);
                return MapMonthResult(month, reportQueue);
            }).ToList();
        }

        public async Task<IEnumerable<SchoolYearMonthResultDTO>> GetMonthsWithoutReportsAsync(Guid yearId)
        {
            var months = await context.SchoolYearMonths
                .AsNoTracking()
                .Where(month =>
                    month.YearId == yearId &&
                    month.Deleted == false)
                .ToListAsync();

            var reportQueuesByMonth = await GetLatestFullMonthReportQueuesAsync(
                months.Select(month => month.Id).ToList());

            return months
                .Select(month =>
                {
                    reportQueuesByMonth.TryGetValue(month.Id, out var reportQueue);
                    return MapMonthResult(month, reportQueue);
                })
                .Where(month =>
                    month.ReportStatus == ReportStatusEnum.NotCreated ||
                    month.ReportStatus == ReportStatusEnum.Returned)
                .ToList();
        }

        public async Task<IEnumerable<SchoolYearMonth>> GetYearMonthesAsync(Guid yearId)
        {

            var resultQuery = context.SchoolYearMonths.Where(m => m.YearId == yearId && m.Deleted == false);

            return await resultQuery.ToListAsync();
        }

        public async Task<SchoolYearMonthResultDTO?> GetYearMonthAsync(Guid monthId)
        {
            var row =  await context.SchoolYearMonths.FirstOrDefaultAsync(m => m.Id == monthId && m.Deleted == false);

            if (row is null)
            {
                return null;
            }

            var reportQueue = await context.ReportQueues
                .AsNoTracking()
                .Where(queue =>
                    queue.MonthId == row.Id &&
                    queue.GradeId == null &&
                    queue.ClassroomId == null)
                .OrderByDescending(queue => queue.CreatedAt)
                .FirstOrDefaultAsync();

            return MapMonthResult(row, reportQueue);

        }

        private async Task<Dictionary<Guid, ReportQueue>> GetLatestFullMonthReportQueuesAsync(
            IReadOnlyCollection<Guid> monthIds)
        {
            if (monthIds.Count == 0)
            {
                return new Dictionary<Guid, ReportQueue>();
            }

            var queues = await context.ReportQueues
                .AsNoTracking()
                .Where(queue =>
                    queue.MonthId.HasValue &&
                    monthIds.Contains(queue.MonthId.Value) &&
                    queue.GradeId == null &&
                    queue.ClassroomId == null)
                .OrderByDescending(queue => queue.CreatedAt)
                .ToListAsync();

            return queues
                .GroupBy(queue => queue.MonthId!.Value)
                .ToDictionary(
                    group => group.Key,
                    group => group.First());
        }

        private static SchoolYearMonthResultDTO MapMonthResult(
            SchoolYearMonth month,
            ReportQueue? reportQueue)
        {
            return new SchoolYearMonthResultDTO
            {
                Id = month.Id,
                YearId = month.YearId,
                Name = month.Name,
                Description = month.Description,
                IsActive = month.IsActive,
                ReportStatus = MapReportStatus(reportQueue?.Status),
                ReportQueueId = reportQueue?.Id
            };
        }

        private static ReportStatusEnum MapReportStatus(ReportQueueStatusEnum? status)
        {
            return status switch
            {
                null => ReportStatusEnum.NotCreated,
                ReportQueueStatusEnum.Queued => ReportStatusEnum.Requested,
                ReportQueueStatusEnum.Processing => ReportStatusEnum.Pendnig,
                ReportQueueStatusEnum.PendingReview => ReportStatusEnum.InReview,
                ReportQueueStatusEnum.Published => ReportStatusEnum.Published,
                ReportQueueStatusEnum.Failed => ReportStatusEnum.Returned,
                _ => ReportStatusEnum.NotCreated
            };
        }

    }
}
