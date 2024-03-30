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
    public class SchoolContactQueryService : BaseQueryService<SchoolContact>, ISchoolContactQueryService
    {
        private readonly IBaseQueryRepository<SchoolContact> queryRepository;
        private readonly IBaseSharedRepository<SchoolContact> sharedRepository;

        public SchoolContactQueryService(IBaseQueryRepository<SchoolContact> queryRepository,
            IBaseSharedRepository<SchoolContact> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
