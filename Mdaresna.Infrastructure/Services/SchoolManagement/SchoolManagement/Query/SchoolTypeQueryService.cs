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
    public class SchoolTypeQueryService : BaseQueryService<SchoolType>, ISchoolTypeQueryService
    {
        private readonly IBaseQueryRepository<SchoolType> queryRepository;
        private readonly IBaseSharedRepository<SchoolType> sharedRepository;

        public SchoolTypeQueryService(IBaseQueryRepository<SchoolType> queryRepository,
            IBaseSharedRepository<SchoolType> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
