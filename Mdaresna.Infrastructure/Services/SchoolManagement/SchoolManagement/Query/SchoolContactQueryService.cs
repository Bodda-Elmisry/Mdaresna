using Mdaresna.Doamin.DTOs.SchoolManagement;
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
    public class SchoolContactQueryService : BaseQueryService<SchoolContact>, ISchoolContactQueryService
    {
        private readonly IBaseQueryRepository<SchoolContact> queryRepository;
        private readonly IBaseSharedRepository<SchoolContact> sharedRepository;
        private readonly ISchoolContactQueryRepository schoolContactQueryRepository;

        public SchoolContactQueryService(IBaseQueryRepository<SchoolContact> queryRepository,
            IBaseSharedRepository<SchoolContact> sharedRepository,
            ISchoolContactQueryRepository schoolContactQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
            this.schoolContactQueryRepository = schoolContactQueryRepository;
        }

        public async Task<SchoolContactResultDTO?> GetSchoolContactById(Guid Id)
        {
            return await schoolContactQueryRepository.GetSchoolContactById(Id);
        }

        public async Task<IEnumerable<SchoolContactResultDTO>> GetSchoolContacts(Guid schoolId)
        {
            return await schoolContactQueryRepository.GetSchoolContacts(schoolId);
        }
    }
}
