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
    public class SchoolGradeQueryService : BaseQueryService<SchoolGrade>, ISchoolGradeQueryService
    {
        private readonly IBaseQueryRepository<SchoolGrade> queryRepository;
        private readonly IBaseSharedRepository<SchoolGrade> sharedRepository;

        public SchoolGradeQueryService(IBaseQueryRepository<SchoolGrade> queryRepository,
            IBaseSharedRepository<SchoolGrade> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
