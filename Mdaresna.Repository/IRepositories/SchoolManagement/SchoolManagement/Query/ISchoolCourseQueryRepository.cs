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
    public interface ISchoolCourseQueryRepository : IBaseQueryRepository<SchoolCourse>
    {
        Task<IEnumerable<SchoolCourseResultDTO>> GetCoursesBySchoolIdAsync(Guid schoolId);

        Task<IEnumerable<SchoolCourseResultDTO>> GetCoursesBySchoolIdAndLanguageIDAsync(Guid schoolId, Guid languageId);
        Task<SchoolCourseResultDTO?> GetCourseIDAsync(Guid id);

    }
}
