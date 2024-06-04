using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query
{
    public interface ISchoolGradeQueryRepository : IBaseQueryRepository<SchoolGrade>
    {
        Task<IEnumerable<SchoolGrade>> GetSchoolGradesAsync(Guid SchoolId);
    }
}
