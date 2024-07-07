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
    public class SchoolYearMonthQueryRepository : BaseQueryRepository<SchoolYearMonth>, ISchoolYearMonthQueryRepository
    {
        private readonly AppDbContext context;

        public SchoolYearMonthQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<SchoolYearMonthResultDTO>> GetYearMonthesAsync(Guid yearId, bool? isActive, string name)
        {

            var resultQuery = context.SchoolYearMonths.Where(m => m.YearId == yearId)
                .Select(m => new SchoolYearMonthResultDTO
                {
                    Id = m.Id,
                    YearId = m.YearId,
                    Name = m.Name,
                    Description = m.Description,
                    IsActive = m.IsActive
                });

            if (isActive != null)
                resultQuery = resultQuery.Where(m => m.IsActive == isActive);

            if(!string.IsNullOrEmpty(name))
                resultQuery = resultQuery.Where(m => m.Name.Contains(name));

            return await resultQuery.ToListAsync();
        }

        public async Task<SchoolYearMonthResultDTO?> GetYearMonthAsync(Guid monthId)
        {
            return await context.SchoolYearMonths.Select(m => new SchoolYearMonthResultDTO
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                IsActive = m.IsActive,
                YearId = m.YearId
            }).FirstOrDefaultAsync(m => m.Id == monthId);

        }


    }
}
