using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Query
{
    public class SchoolCourseQueryRepository : BaseQueryRepository<SchoolCourse>, ISchoolCourseQueryRepository
    {
        private readonly AppDbContext context;

        public SchoolCourseQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<SchoolCourse>> GetCoursesBySchoolIdAsync(Guid schoolId)
        {
            return await context.SchoolCourses.Where(c => c.SchoolId == schoolId).ToListAsync();
        }

        public async Task<IEnumerable<SchoolCourse>> GetCoursesBySchoolIdAndLanguageIDAsync(Guid schoolId, Guid languageId)
        {
            return await context.SchoolCourses.Where(c => c.SchoolId == schoolId && c.LanguageId == languageId).ToListAsync();
        }



    }
}
