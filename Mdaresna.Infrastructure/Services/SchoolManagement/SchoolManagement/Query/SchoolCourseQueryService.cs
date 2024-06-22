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
    public class SchoolCourseQueryService : BaseQueryService<SchoolCourse>, ISchoolCourseQueryService
    {
        private readonly IBaseQueryRepository<SchoolCourse> queryRepository;
        private readonly IBaseSharedRepository<SchoolCourse> sharedRepository;
        private readonly ISchoolCourseQueryRepository schoolCourseQueryRepository;

        public SchoolCourseQueryService(IBaseQueryRepository<SchoolCourse> queryRepository,
            IBaseSharedRepository<SchoolCourse> sharedRepository,
            ISchoolCourseQueryRepository schoolCourseQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
            this.schoolCourseQueryRepository = schoolCourseQueryRepository;
        }
        public async Task<IEnumerable<SchoolCourse>> GetCoursesBySchoolIdAsync(Guid schoolId)
        {
            return await schoolCourseQueryRepository.GetCoursesBySchoolIdAsync(schoolId);
        }

        public async Task<IEnumerable<SchoolCourse>> GetCoursesBySchoolIdAndLanguageIDAsync(Guid schoolId, Guid languageId)
        {
            return await schoolCourseQueryRepository.GetCoursesBySchoolIdAndLanguageIDAsync(schoolId, languageId);
        }




    }
}
