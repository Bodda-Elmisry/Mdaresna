using Mdaresna.Doamin.DTOs.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query
{
    public interface IClassRoomTeacherCourseQueryService : IBaseQueryService<ClassRoomTeacherCourse>
    {
        Task<ClassRoomTeacherCourseInitialDTO> GetInitialDataAsync(Guid schoolId);
        Task<IEnumerable<ClassRoomTeacherCourseResultDTO>> GetTeacherDataAsync(Guid teacherId);
        Task<ClassRoomTeacherCourseResultDTO?> GetClassRoomIeacherCourseAsync(Guid teacherId, Guid roomId, Guid courseId);
        Task<IEnumerable<ClassRoomTeacherCourseResultDTO>?> GetTeacherClassroomsCoursesAsync(Guid teacherId, Guid schoolId, Guid? roomId, Guid? courseId);
        Task<IEnumerable<ClassRoomTeacherCourse>?> GetTeacherClassroomsCoursesAsync(Guid teacherId, Guid schoolId);
        Task<IEnumerable<ClassRoomTeacherCourseResultDTO>?> GetClassRoomIeacherCoursesAsync(Guid teacherId, Guid roomId);
        Task<ClassRoomTeacherCourse?> GetByIdAsync(Guid teacherId, Guid roomId, Guid courseId);
        Task<bool> IsExistAsync(Guid teacherId, Guid roomId, Guid courseId);
    }
}
