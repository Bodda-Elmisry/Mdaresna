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
    public class SchoolYearMonthQueryService : BaseQueryService<SchoolYearMonth>, ISchoolYearMonthQueryService
    {
        private readonly ISchoolYearMonthQueryRepository schoolYearMonthQueryRepository;

        public SchoolYearMonthQueryService(IBaseQueryRepository<SchoolYearMonth> queryRepository,
            IBaseSharedRepository<SchoolYearMonth> sharedRepository,
            ISchoolYearMonthQueryRepository schoolYearMonthQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.schoolYearMonthQueryRepository = schoolYearMonthQueryRepository;
        }

        public async Task<SchoolYearMonthResultDTO?> GetYearMonthAsync(Guid monthId)
        {
            return await schoolYearMonthQueryRepository.GetYearMonthAsync(monthId);
        }

        public async Task<IEnumerable<SchoolYearMonthResultDTO>> GetYearMonthesAsync(Guid yearId, bool? isActive, string name)
        {
            return await schoolYearMonthQueryRepository.GetYearMonthesAsync(yearId, isActive, name);
        }
    }
}
