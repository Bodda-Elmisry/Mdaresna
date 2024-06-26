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
        Task<IEnumerable<Student>> GetStudentsBySchoolIdAsync(Guid schoolId);
        Task<IEnumerable<Student>> GetStudentsBySchoolIdAndClassRoomIdAsync(Guid schoolId, Guid classroomId);
    }
}
