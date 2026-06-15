using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Repository.IRepositories.Base;

namespace Mdaresna.Repository.IRepositories.SettingsManagement.Query
{
    public interface IReportQueueQueryRepository : IBaseQueryRepository<ReportQueue>
    {
        Task<IEnumerable<ReportQueue>> GetPendingAsync(int take);

        Task<IEnumerable<ReportQueue>> GetBySchoolAsync(
            Guid schoolId,
            Guid? monthId = null,
            Guid? gradeId = null,
            Guid? classroomId = null,
            ReportQueueStatusEnum? status = null,
            StudentReportTypesEnum? reportType = null);

        Task<PagedResultDTO<ReportQueue>> GetBySchoolPagedAsync(
            Guid schoolId,
            Guid? monthId = null,
            Guid? gradeId = null,
            Guid? classroomId = null,
            ReportQueueStatusEnum? status = null,
            StudentReportTypesEnum? reportType = null,
            int pageNumber = 1,
            int? pageSize = null);

        Task<ReportQueue?> GetByIdWithDetailsAsync(Guid id);
    }
}
