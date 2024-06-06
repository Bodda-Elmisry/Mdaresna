using Mdaresna.Doamin.Models.AdminManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query
{
    public interface IClassRoomLanguageQueryService : IBaseQueryService<ClassRoomLanguage>
    {
        public Task<IEnumerable<School>> GetLanguageSchools(Guid LnaguageId);
        public Task<IEnumerable<Language>> GetSchoolLanguages(Guid SchoolId);

    }
}
