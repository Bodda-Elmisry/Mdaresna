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
    public class SchoolContactTypeQueryService : BaseQueryService<SchoolContactType>, ISchoolContactTypeQueryService
    {
        private readonly IBaseQueryRepository<SchoolContactType> queryRepository;
        private readonly IBaseSharedRepository<SchoolContactType> sharedRepository;

        public SchoolContactTypeQueryService(IBaseQueryRepository<SchoolContactType> queryRepository,
            IBaseSharedRepository<SchoolContactType> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
