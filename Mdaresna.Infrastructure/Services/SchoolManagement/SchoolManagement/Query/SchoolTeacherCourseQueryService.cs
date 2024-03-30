using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
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

        public SchoolTeacherCourseQueryService(IBaseQueryRepository<SchoolTeacherCourse> queryRepository,
            IBaseSharedRepository<SchoolTeacherCourse> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
