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
    public interface IStudentAttendanceQueryService : IBaseQueryService<StudentAttendance>
    {
        Task<IEnumerable<StudentAttendanceResultDTO>> GetStudentsAttendancesAsync(Guid? studentId, Guid? classRoomId, int pageNumber);
    }
}
