using Mdaresna.Doamin.DTOs.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.ClassRoomManagement.Query
{
    public class ClassRoomTeacherCourseQueryService : BaseQueryService<ClassRoomTeacherCourse>, IClassRoomTeacherCourseQueryService
    {
        private readonly IBaseQueryRepository<ClassRoomTeacherCourse> queryRepository;
        private readonly IBaseSharedRepository<ClassRoomTeacherCourse> sharedRepository;
        private readonly IClassRoomTeacherCourseQueryRepository classRoomTeacherCourseQueryRepository;

        public ClassRoomTeacherCourseQueryService(IBaseQueryRepository<ClassRoomTeacherCourse> queryRepository,
            IBaseSharedRepository<ClassRoomTeacherCourse> sharedRepository,
            IClassRoomTeacherCourseQueryRepository classRoomTeacherCourseQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
            this.classRoomTeacherCourseQueryRepository = classRoomTeacherCourseQueryRepository;
        }

        public async Task<ClassRoomTeacherCourse?> GetByIdAsync(Guid teacherId, Guid roomId, Guid courseId)
        {
            return await classRoomTeacherCourseQueryRepository.GetByIdAsync(teacherId, roomId, courseId);
        }

        public async Task<ClassRoomTeacherCourseResultDTO?> GetClassRoomIeacherCourseAsync(Guid teacherId, Guid roomId, Guid courseId)
        {
            return await classRoomTeacherCourseQueryRepository.GetClassRoomIeacherCourseAsync(teacherId, roomId, courseId);
        }

        public async Task<IEnumerable<ClassRoomTeacherCourseResultDTO>?> GetClassRoomIeacherCoursesAsync(Guid teacherId, Guid roomId)
        {
            return await classRoomTeacherCourseQueryRepository.GetClassRoomIeacherCoursesAsync(teacherId, roomId);
        }

        public async Task<ClassRoomTeacherCourseInitialDTO> GetInitialDataAsync(Guid schoolId)
        {
            return await classRoomTeacherCourseQueryRepository.GetInitialDataAsync(schoolId);
        }

        public async Task<IEnumerable<ClassRoomTeacherCourseResultDTO>> GetTeacherDataAsync(Guid teacherId)
        {
            return await classRoomTeacherCourseQueryRepository.GetTeacherDataAsync(teacherId);
        }

        public async Task<bool> IsExistAsync(Guid teacherId, Guid roomId, Guid courseId)
        {
            return await classRoomTeacherCourseQueryRepository.IsExistAsync(teacherId,roomId, courseId);
        }
    }
}
