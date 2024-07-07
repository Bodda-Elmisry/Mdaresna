using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query
{
    public interface ISchoolExamRateHeaderQueryService : IBaseQueryService<SchoolExamRateHeader>
    {
        Task<IEnumerable<SchoolExamRateHeaderResultDTO>> GetRateHeadersAsync(Guid schoolId, string name, decimal? percentage);
        Task<SchoolExamRateHeaderResultDTO?> GetRateHeaderAsync(Guid headerId);
    }
}
