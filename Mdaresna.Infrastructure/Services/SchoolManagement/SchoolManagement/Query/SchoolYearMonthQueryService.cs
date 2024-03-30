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
    public class SchoolYearMonthQueryService : BaseQueryService<SchoolYearMonth>, ISchoolYearMonthQueryService
    {
        private readonly IBaseQueryRepository<SchoolYearMonth> queryRepository;
        private readonly IBaseSharedRepository<SchoolYearMonth> sharedRepository;

        public SchoolYearMonthQueryService(IBaseQueryRepository<SchoolYearMonth> queryRepository,
            IBaseSharedRepository<SchoolYearMonth> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
