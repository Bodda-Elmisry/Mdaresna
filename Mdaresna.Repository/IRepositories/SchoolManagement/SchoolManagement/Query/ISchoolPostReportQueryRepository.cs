using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query
{
    public interface ISchoolPostReportQueryRepository : IBaseQueryRepository<SchoolPostReport>
    {
        Task<IEnumerable<SchoolPostReportResultDTO>> GetSchoolPostReportsAsync(Guid schoolId);
    }
}
