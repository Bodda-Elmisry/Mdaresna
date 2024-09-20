using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query
{
    public interface ISchoolContactQueryRepository : IBaseQueryRepository<SchoolContact>
    {
        Task<IEnumerable<SchoolContactResultDTO>> GetSchoolContacts(Guid schoolId);
        Task<SchoolContactResultDTO?> GetSchoolContactById(Guid Id);
    }
}
