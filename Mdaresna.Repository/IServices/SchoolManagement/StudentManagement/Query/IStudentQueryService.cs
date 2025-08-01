using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query
{
    public interface IStudentQueryService : IBaseQueryService<Student>
    {
        Task<IEnumerable<Student>> GetStudentsBySchoolIdAsync(Guid schoolId, bool? ispayed);
        Task<IEnumerable<StudentResultDTO>> GetStudentsBySchoolIdViewAsync(Guid schoolId, string studentCode, string studentName);
        Task<IEnumerable<StudentResultDTO>> GetStudentsBySchoolIdAndClassRoomIdAsync(Guid schoolId, Guid classroomId);
        Task<StudentResultDTO?> GetStudentByIdAsync(Guid studentId);
        Task<StudentResultDTO?> GetStudentByCodeAsync(string code);
        public Task<string> GetMaxStudebtCodeAsync(Guid schoolId);
    }
}
