using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Repository.IRepositories.SettingsManagement.Query;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Infrastructure.Repositories.SettingsManagement.Query
{
    public class ReportQueueQueryRepository : IReportQueueQueryRepository
    {
        private readonly AppDbContext context;

        public ReportQueueQueryRepository(AppDbContext context)
        {
            this.context = context;
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

        public async Task<IEnumerable<ReportQueue>> GetBySchoolAsync(Guid schoolId)
        {
            return await GetQueueWithDetails()
                .Where(q => q.SchoolId == schoolId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        private IQueryable<ReportQueue> GetQueueWithDetails()
        {
            return context.ReportQueues
                .Include(q => q.School)
                .Include(q => q.Grade)
                .Include(q => q.Classroom)
                .Include(q => q.Month)
                .Include(q => q.ReviewdBy)
                .Include(q => q.CreatedBy);
        }
    }
}
