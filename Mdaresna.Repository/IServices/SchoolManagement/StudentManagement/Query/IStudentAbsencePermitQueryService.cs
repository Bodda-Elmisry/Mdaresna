using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Repository.IServices.Base;

namespace Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query
{
    public interface IStudentAbsencePermitQueryService : IBaseQueryService<StudentAbsencePermit>
    {
        Task<StudentAbsencePermit?> GetActivePermitAsync(Guid studentId, DateTime date);
        Task<IEnumerable<StudentAbsencePermitResultDTO>> GetStudentAbsencePermitsAsync(
            Guid? studentId,
            Guid? parentId,
            int pageNumber);
    }
}
