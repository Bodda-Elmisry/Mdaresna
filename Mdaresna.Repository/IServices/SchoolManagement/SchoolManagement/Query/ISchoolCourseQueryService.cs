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
    public interface ISchoolCourseQueryService : IBaseQueryService<SchoolCourse>
    {
        Task<IEnumerable<SchoolCourseResultDTO>> GetCoursesBySchoolIdAsync(Guid schoolId);

        Task<IEnumerable<SchoolCourseResultDTO>> GetCoursesBySchoolIdAndLanguageIDAsync(Guid schoolId, Guid languageId);
    }
}
