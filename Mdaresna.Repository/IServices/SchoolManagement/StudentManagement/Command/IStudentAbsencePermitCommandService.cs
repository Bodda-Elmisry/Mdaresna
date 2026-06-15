using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Repository.IServices.Base;

namespace Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command
{
    public interface IStudentAbsencePermitCommandService : IBaseCommandService<StudentAbsencePermit>
    {
        Task<string> CreateAbsencePermitAsync(AddStudentAbsencePermitDTO permitDTO);
        Task<bool> SoftDeleteAbsencePermitAsync(Guid permitId, Guid parentId);
        Task<string> ReviewAbsencePermitAsync(ReviewStudentAbsencePermitDTO reviewDTO);
    }
}
