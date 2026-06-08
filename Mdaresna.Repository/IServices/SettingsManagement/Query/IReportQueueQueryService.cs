using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Repository.IServices.Base;

namespace Mdaresna.Repository.IServices.SettingsManagement.Query
{
    public interface IReportQueueQueryService : IBaseQueryService<ReportQueue>
    {
        Task<IEnumerable<ReportQueue>> GetPendingAsync(int take);

        Task<IEnumerable<ReportQueue>> GetBySchoolAsync(Guid schoolId);

        Task<ReportQueue?> GetByIdWithDetailsAsync(Guid id);
    }
}
