using Mdaresna.Doamin.DTOs.ReportingDTOs.StudentWeeklyReportDTOs;

namespace Mdaresna.Repository.IServices
{
    public interface IReportingService
    {
        Task<StudentWeeklyReportResponseDTO?> GetStudentWeeklyReport(StudentWeeklyReportRequestDTO Request);
    }
}
