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
    public class SchoolPostQueryService : BaseQueryService<SchoolPost>, ISchoolPostQueryService
    {
        private readonly IBaseQueryRepository<SchoolPost> queryRepository;
        private readonly IBaseSharedRepository<SchoolPost> sharedRepository;

        public SchoolPostQueryService(IBaseQueryRepository<SchoolPost> queryRepository,
            IBaseSharedRepository<SchoolPost> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
