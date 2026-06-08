using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SettingsManagement.Query;
using Mdaresna.Repository.IServices.SettingsManagement.Query;

namespace Mdaresna.Infrastructure.Services.SettingsManagement.Query
{
    public class ReportQueueQueryService : IReportQueueQueryService
    {
        private readonly IReportQueueQueryRepository reportQueueQueryRepository;
        private readonly IBaseSharedRepository<ReportQueue> sharedRepository;

        public ReportQueueQueryService(
            IReportQueueQueryRepository reportQueueQueryRepository,
            IBaseSharedRepository<ReportQueue> sharedRepository)
        {
            this.reportQueueQueryRepository = reportQueueQueryRepository;
            this.sharedRepository = sharedRepository;
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

        public async Task<IEnumerable<ReportQueue>> GetBySchoolAsync(Guid schoolId)
        {
            return await reportQueueQueryRepository.GetBySchoolAsync(schoolId);
        }
    }
}
