using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query
{
    public interface ISchoolTeacherCourseQueryService : IBaseQueryService<SchoolTeacherCourse>
    {
        Task<IEnumerable<SchoolCourse>> GetSchoolTeacherCourcesAsync(Guid schoolId, Guid teacherId);
        Task<SchoolTeacherCourse?> GetSchoolTeacherCourceAsync(Guid schoolId, Guid teacherId, Guid courseId);
        Task<SchoolTeacherCourseInitialDTO> GetSchoolTeacherCourceInitialListsAsync(Guid schoolId);
    }
}
