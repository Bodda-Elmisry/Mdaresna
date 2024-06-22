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
    public interface ISchoolContactQueryService : IBaseQueryService<SchoolContact>
    {
        Task<IEnumerable<SchoolContactResultDTO>> GetSchoolContacts(Guid schoolId);
    }
}
