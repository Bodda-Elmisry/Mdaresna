using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
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
        private readonly ISchoolContactTypeQueryRepository schoolContactTypeQueryRepository;

        public SchoolContactTypeQueryService(IBaseQueryRepository<SchoolContactType> queryRepository,
            IBaseSharedRepository<SchoolContactType> sharedRepository,
            ISchoolContactTypeQueryRepository schoolContactTypeQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.schoolContactTypeQueryRepository = schoolContactTypeQueryRepository;
        }
    }
}
