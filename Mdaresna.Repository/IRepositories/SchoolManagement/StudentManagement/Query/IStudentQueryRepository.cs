using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query
{
    public interface IStudentQueryRepository : IBaseQueryRepository<Student>
    {
        Task<IEnumerable<StudentResultDTO>> GetStudentsBySchoolIdAsync(Guid schoolId);
        Task<StudentResultDTO?> GetStudentByIdAsync(Guid studentId);
        Task<IEnumerable<StudentResultDTO>> GetStudentsBySchoolIdAndClassRoomIdAsync(Guid schoolId, Guid classroomId);

        public Task<string> GetMaxStudebtCodeAsync(Guid schoolId);
    }
}
