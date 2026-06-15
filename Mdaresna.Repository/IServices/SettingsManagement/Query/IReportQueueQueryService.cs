using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.DTOs.ReportingDTOs;
using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Repository.IServices.Base;

namespace Mdaresna.Repository.IServices.SettingsManagement.Query
{
    public interface IReportQueueQueryService : IBaseQueryService<ReportQueue>
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

        Task<IReadOnlyList<StudentReportResultDTO>> GetStudentReportsByReportIdAsync(
            Guid reportQueueId,
            CancellationToken cancellationToken = default);
    }
}
