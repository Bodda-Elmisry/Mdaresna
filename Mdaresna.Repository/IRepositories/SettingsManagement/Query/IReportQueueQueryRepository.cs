using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Repository.IRepositories.Base;

namespace Mdaresna.Repository.IRepositories.SettingsManagement.Query
{
    public interface IReportQueueQueryRepository : IBaseQueryRepository<ReportQueue>
    {
        Task<IEnumerable<ReportQueue>> GetPendingAsync(int take);

        Task<IEnumerable<ReportQueue>> GetBySchoolAsync(Guid schoolId);

        Task<ReportQueue?> GetByIdWithDetailsAsync(Guid id);
    }
}
