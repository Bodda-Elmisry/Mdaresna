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
    public class SchoolPostImageQueryService : BaseQueryService<SchoolPostImage>, ISchoolPostImageQueryService
    {
        private readonly IBaseQueryRepository<SchoolPostImage> queryRepository;
        private readonly IBaseSharedRepository<SchoolPostImage> sharedRepository;

        public SchoolPostImageQueryService(IBaseQueryRepository<SchoolPostImage> queryRepository,
            IBaseSharedRepository<SchoolPostImage> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
