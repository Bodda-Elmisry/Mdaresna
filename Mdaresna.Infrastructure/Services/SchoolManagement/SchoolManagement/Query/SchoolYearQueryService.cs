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
    public class SchoolYearQueryService : BaseQueryService<SchoolYear>, ISchoolYearQueryService
    {
        private readonly IBaseQueryRepository<SchoolYear> queryRepository;
        private readonly IBaseSharedRepository<SchoolYear> sharedRepository;

        public SchoolYearQueryService(IBaseQueryRepository<SchoolYear> queryRepository,
            IBaseSharedRepository<SchoolYear> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
