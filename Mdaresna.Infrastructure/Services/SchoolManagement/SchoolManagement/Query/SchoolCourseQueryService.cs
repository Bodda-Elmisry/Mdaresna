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
    public class SchoolCourseQueryService : BaseQueryService<SchoolCourse>, ISchoolCourseQueryService
    {
        private readonly IBaseQueryRepository<SchoolCourse> queryRepository;
        private readonly IBaseSharedRepository<SchoolCourse> sharedRepository;

        public SchoolCourseQueryService(IBaseQueryRepository<SchoolCourse> queryRepository,
            IBaseSharedRepository<SchoolCourse> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
