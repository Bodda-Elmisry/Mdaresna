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
    public class SchoolQueryRepository : BaseQueryRepository<School>, ISchoolQueryRepository
    {
        private readonly AppDbContext context;

        public SchoolQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<School>> GetUserAdminSchools(Guid userId)
        {
            return await context.Schools.Where(s => s.SchoolAdminId == userId).ToListAsync();
        }
    }
}
