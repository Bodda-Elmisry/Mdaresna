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
    public interface ISchoolYearQueryService : IBaseQueryService<SchoolYear>
    {
        Task<SchoolYearResultDTO> GetCurrentYearAsync(Guid schoolId);
        Task<IEnumerable<SchoolYearResultDTO>> GetSchoolYearsAsync(Guid schoolId);
    }
}
