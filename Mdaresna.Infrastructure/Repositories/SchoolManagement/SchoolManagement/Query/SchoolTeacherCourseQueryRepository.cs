using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Query
{
    public class SchoolTeacherCourseQueryRepository : BaseQueryRepository<SchoolTeacherCourse>, ISchoolTeacherCourseQueryRepository
    {
       public SchoolTeacherCourseQueryRepository(AppDbContext context) : base(context)
        {
        }
    }
}
