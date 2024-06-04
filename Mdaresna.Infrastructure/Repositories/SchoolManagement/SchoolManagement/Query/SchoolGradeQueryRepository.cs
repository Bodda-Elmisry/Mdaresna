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
    public class SchoolGradeQueryRepository : BaseQueryRepository<SchoolGrade>, ISchoolGradeQueryRepository
    {
        private readonly AppDbContext context;

        public SchoolGradeQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<SchoolGrade>> GetSchoolGradesAsync(Guid SchoolId)
        {
            return await context.SchoolGrades.Where(g => g.SchoolId == SchoolId).ToListAsync();
        }
    }
}
