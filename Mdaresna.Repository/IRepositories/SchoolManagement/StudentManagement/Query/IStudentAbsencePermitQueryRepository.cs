using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Repository.IRepositories.Base;

namespace Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query
{
    public interface IStudentAbsencePermitQueryRepository : IBaseQueryRepository<StudentAbsencePermit>
    {
        Task<StudentAbsencePermit?> GetActivePermitAsync(Guid studentId, DateTime date);
        Task<IEnumerable<StudentAbsencePermitResultDTO>> GetStudentAbsencePermitsAsync(
            Guid? studentId,
            Guid? parentId,
            int pageNumber);
    }
}
