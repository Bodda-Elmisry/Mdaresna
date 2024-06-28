using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Query
{
    public class SchoolTeacherCourseQueryService : BaseQueryService<SchoolTeacherCourse>, ISchoolTeacherCourseQueryService
    {
        private readonly IBaseQueryRepository<SchoolTeacherCourse> queryRepository;
        private readonly IBaseSharedRepository<SchoolTeacherCourse> sharedRepository;
        private readonly ISchoolTeacherCourseQueryRepository schoolTeacherCourseQueryRepository;

        public SchoolTeacherCourseQueryService(IBaseQueryRepository<SchoolTeacherCourse> queryRepository,
            IBaseSharedRepository<SchoolTeacherCourse> sharedRepository,
            ISchoolTeacherCourseQueryRepository schoolTeacherCourseQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
            this.schoolTeacherCourseQueryRepository = schoolTeacherCourseQueryRepository;
        }

        public async Task<SchoolTeacherCourse?> GetSchoolTeacherCourceAsync(Guid schoolId, Guid teacherId, Guid courseId)
        {
            return await schoolTeacherCourseQueryRepository.GetSchoolTeacherCourceAsync(schoolId, teacherId, courseId);
        }

        public async Task<IEnumerable<SchoolCourse>> GetSchoolTeacherCourcesAsync(Guid schoolId, Guid teacherId)
        {
            return await schoolTeacherCourseQueryRepository.GetSchoolTeacherCourcesAsync(schoolId, teacherId);
        }

        public async Task<SchoolTeacherCourseInitialDTO> GetSchoolTeacherCourceInitialListsAsync(Guid schoolId)
        {
            return await schoolTeacherCourseQueryRepository.GetSchoolTeacherCourceInitialListsAsync(schoolId);
        }

    }
}
