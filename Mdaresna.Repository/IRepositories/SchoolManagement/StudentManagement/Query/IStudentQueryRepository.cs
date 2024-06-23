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
        Task<IEnumerable<Student>> GetStudentsBySchoolIdAsync(Guid schoolId);
        Task<IEnumerable<Student>> GetStudentsBySchoolIdAndClassRoomIdAsync(Guid schoolId, Guid classroomId);
    }
}
