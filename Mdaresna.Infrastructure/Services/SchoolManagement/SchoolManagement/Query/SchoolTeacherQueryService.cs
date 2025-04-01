using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Query
{
    public class SchoolTeacherQueryService : BaseQueryService<SchoolTeacher>, ISchoolTeacherQueryService
    {
        private readonly ISchoolTeacherQueryRepository schoolTeacherQueryRepository;

        public SchoolTeacherQueryService(IBaseQueryRepository<SchoolTeacher> queryRepository,
            IBaseSharedRepository<SchoolTeacher> sharedRepository,
            ISchoolTeacherQueryRepository schoolTeacherQueryRepository)
            :base(queryRepository, sharedRepository)
        {
            this.schoolTeacherQueryRepository = schoolTeacherQueryRepository;
        }

        public async Task<SchoolTeacher> GetSchoolTeacherByIdAsync(Guid schoolId, Guid teacherId)
        {
            return await schoolTeacherQueryRepository.GetSchoolTeacherByIdAsync(schoolId, teacherId);
        }

        public async Task<IEnumerable<TeacherResultDTO>> GetSchoolTeachersAsync(Guid schoolId, string teacherName, string teacherPhoneNumber, string teacherEmail)
        {
            return await schoolTeacherQueryRepository.GetSchoolTeachersAsync(schoolId, teacherName, teacherPhoneNumber, teacherEmail);
        }

        public async Task<IEnumerable<TeacherSchoolResultDTO>> GetTeacherSchoolsAsync(Guid teacherId)
        {
            return await schoolTeacherQueryRepository.GetTeacherSchoolsAsync(teacherId);
        }
    }
}
