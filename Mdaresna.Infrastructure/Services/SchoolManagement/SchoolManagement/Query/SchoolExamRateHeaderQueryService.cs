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
    public class SchoolExamRateHeaderQueryService : BaseQueryService<SchoolExamRateHeader>, ISchoolExamRateHeaderQueryService
    {
        private readonly IBaseQueryRepository<SchoolExamRateHeader> queryRepository;
        private readonly IBaseSharedRepository<SchoolExamRateHeader> sharedRepository;

        public SchoolExamRateHeaderQueryService(IBaseQueryRepository<SchoolExamRateHeader> queryRepository,
            IBaseSharedRepository<SchoolExamRateHeader> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
