using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query
{
    public interface ISchoolTeacherCourseQueryRepository : IBaseQueryRepository<SchoolTeacherCourse>
    {
        Task<IEnumerable<SchoolTeacherCourseResultDTO>> GetSchoolTeacherCourcesAsync(Guid schoolId, Guid teacherId);
        Task<SchoolTeacherCourseResultDTO?> GetSchoolTeacherCourceAsync(Guid schoolId, Guid teacherId, Guid courseId);
        Task<SchoolTeacherCourseInitialDTO> GetSchoolTeacherCourceInitialListsAsync(Guid schoolId);
    }
}
