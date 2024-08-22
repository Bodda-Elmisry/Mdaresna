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
    public interface IStudentAttendanceQueryRepository : IBaseQueryRepository<StudentAttendance>
    {
        Task<IEnumerable<StudentAttendanceResultDTO>> GetStudentsAttendancesAsync(Guid? studentId, Guid? classRoomId, int pageNumber);
    }
}
