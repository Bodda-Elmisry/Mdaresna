using Mdaresna.Doamin.Models.AdminManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query
{
    public interface IClassRoomLanguageQueryRepository : IBaseQueryRepository<ClassRoomLanguage>
    {
        public Task<IEnumerable<School>> GetLanguageSchools(Guid LnaguageId);
        public Task<IEnumerable<Language>> GetSchoolLanguages(Guid LnaguageId);
        Task<ClassRoomLanguage?> GetSchoolLanguageById(Guid SchoolId, Guid LanguageId);
    }
}
