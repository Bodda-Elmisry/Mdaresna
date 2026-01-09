using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query
{
    public interface ISchoolPostReportQueryService : IBaseQueryService<SchoolPostReport>
    {
        Task<IEnumerable<SchoolPostReportResultDTO>> GetSchoolPostReportsAsync(Guid schoolId);
    }
}
