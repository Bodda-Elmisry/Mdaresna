using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mdaresna.Doamin.DTOs.SchoolManagement;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Query
{
    public class SchoolPostReportQueryService : BaseQueryService<SchoolPostReport>, ISchoolPostReportQueryService
    {
        private readonly ISchoolPostReportQueryRepository schoolPostReportQueryRepository;

        public SchoolPostReportQueryService(IBaseQueryRepository<SchoolPostReport> queryRepository,
            IBaseSharedRepository<SchoolPostReport> sharedRepository,
            ISchoolPostReportQueryRepository schoolPostReportQueryRepository) : base(queryRepository, sharedRepository)
        {
            this.schoolPostReportQueryRepository = schoolPostReportQueryRepository;
        }

        public async Task<IEnumerable<SchoolPostReportResultDTO>> GetSchoolPostReportsAsync(Guid schoolId)
        {
            return await schoolPostReportQueryRepository.GetSchoolPostReportsAsync(schoolId);
        }
    }
}
