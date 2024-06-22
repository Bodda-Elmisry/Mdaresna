using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query
{
    public interface ISchoolCourseQueryRepository : IBaseQueryRepository<SchoolCourse>
    {
        Task<IEnumerable<SchoolCourse>> GetCoursesBySchoolIdAsync(Guid schoolId);

        Task<IEnumerable<SchoolCourse>> GetCoursesBySchoolIdAndLanguageIDAsync(Guid schoolId, Guid languageId);

    }
}
