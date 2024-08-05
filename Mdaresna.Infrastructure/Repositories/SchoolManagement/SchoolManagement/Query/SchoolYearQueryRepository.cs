using Mdaresna.Doamin.DTOs.SchoolManagement;
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
    public class SchoolYearQueryRepository : BaseQueryRepository<SchoolYear>, ISchoolYearQueryRepository
    {
        private readonly AppDbContext context;

        public SchoolYearQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }


        public async Task<SchoolYearResultDTO?> GetCurrentYearAsync(Guid schoolId)
        {
            var year = await context.SchoolYears
                            .OrderBy(y => y.CreateDate)
                            .FirstOrDefaultAsync(y => y.SchoolId == schoolId && y.IsActive == true && y.Compleated == false);

            return year == null ? 
                null :
                new SchoolYearResultDTO
            {
                Id = year.Id,
                Name = year.Name,
                Description = year.Description,
                Active = year.IsActive,
                Completed = year.Compleated,
                SchoolId = year.SchoolId
            };
        }

        public async Task<IEnumerable<SchoolYearResultDTO>> GetSchoolYearsAsync(Guid schoolId)
        {
            var years = await context.SchoolYears.Where(y => y.SchoolId == schoolId).Select(y => new SchoolYearResultDTO
            {
                Id = y.Id,
                Name = y.Name,
                Description = y.Description,
                Completed = y.Compleated,
                SchoolId = y.SchoolId
            }).ToListAsync();

            return years;

        }
    }
}
