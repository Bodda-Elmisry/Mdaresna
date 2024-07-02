using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Query;
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
    public class SchoolYearQueryService : BaseQueryService<SchoolYear>, ISchoolYearQueryService
    {
        private readonly IBaseQueryRepository<SchoolYear> queryRepository;
        private readonly IBaseSharedRepository<SchoolYear> sharedRepository;
        private readonly ISchoolYearQueryRepository schoolYearQueryRepository;

        public SchoolYearQueryService(IBaseQueryRepository<SchoolYear> queryRepository,
            IBaseSharedRepository<SchoolYear> sharedRepository,
            ISchoolYearQueryRepository schoolYearQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
            this.schoolYearQueryRepository = schoolYearQueryRepository;
        }

        

        public async Task<SchoolYearResultDTO> GetCurrentYearAsync()
        {
            return await schoolYearQueryRepository.GetCurrentYearAsync();
        }

        public async Task<IEnumerable<SchoolYearResultDTO>> GetSchoolYearsAsync(Guid schoolId)
        {
            return await schoolYearQueryRepository.GetSchoolYearsAsync(schoolId);
        }
    }
}
