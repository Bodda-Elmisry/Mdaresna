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
    public class SchoolExamRateHeaderQueryService : BaseQueryService<SchoolExamRateHeader>, ISchoolExamRateHeaderQueryService
    {
        private readonly ISchoolExamRateHeaderQueryRepository schoolExamRateHeaderQueryRepository;

        public SchoolExamRateHeaderQueryService(IBaseQueryRepository<SchoolExamRateHeader> queryRepository,
            IBaseSharedRepository<SchoolExamRateHeader> sharedRepository,
            ISchoolExamRateHeaderQueryRepository schoolExamRateHeaderQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.schoolExamRateHeaderQueryRepository = schoolExamRateHeaderQueryRepository;
        }

        public async Task<SchoolExamRateHeaderResultDTO?> GetRateHeaderAsync(Guid headerId)
        {
            return await schoolExamRateHeaderQueryRepository.GetRateHeaderAsync(headerId);
        }

        public async Task<IEnumerable<SchoolExamRateHeaderResultDTO>> GetRateHeadersAsync(Guid schoolId, string name, decimal? percentage)
        {
            return await schoolExamRateHeaderQueryRepository.GetRateHeadersAsync(schoolId, name, percentage);
        }
    }
}
