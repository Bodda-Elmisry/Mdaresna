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
    public interface ISchoolQueryService : IBaseQueryService<School>
    {
        Task<IEnumerable<SchoolResultDTO>> GetUserAdminSchools(Guid userId);
        Task<IEnumerable<SchoolResultDTO>> GetUserSchools(Guid userId, bool? active = null);
        Task<SchoolResultDTO?> GetSchoolById(Guid schoolId);
        Task<IEnumerable<SchoolResultDTO>> GetSchoolsList(string? name, bool? active, Guid? schoolTypeId, Guid? coinTypeId, Guid? adminId, int pageNumber, bool? getNewSchools);
        Task<IEnumerable<Guid>> GetSchoolUsersIds(Guid schoolId);
    }
}
