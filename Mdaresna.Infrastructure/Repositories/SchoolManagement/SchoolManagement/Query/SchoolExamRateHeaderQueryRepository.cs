using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Query
{
    public class SchoolExamRateHeaderQueryRepository : BaseQueryRepository<SchoolExamRateHeader>, ISchoolExamRateHeaderQueryRepository
    {
        private readonly AppDbContext context;

        public SchoolExamRateHeaderQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<SchoolExamRateHeaderResultDTO>> GetRateHeadersAsync(Guid schoolId, string name, decimal? percentage)
        {
            var query = context.SchoolExamRateHeaders.Where(h => h.SchoolId == schoolId && h.Deleted == false);

            if(!string.IsNullOrEmpty(name)) 
                query = query.Where(h=> h.Name.Contains(name));

            if(percentage != null)
                query = query.Where(h=> h.Percentage == percentage);

            return await query.Select(h => new SchoolExamRateHeaderResultDTO
            {
                Id = h.Id,
                Name = h.Name,
                Percentage = h.Percentage,
                Note = h.Note,
                SchoolId = h.SchoolId
            }).ToListAsync();
                
        }

        public async Task<SchoolExamRateHeaderResultDTO?> GetRateHeaderAsync(Guid headerId)
        {
            var row = await context.SchoolExamRateHeaders.FirstOrDefaultAsync(h => h.Id == headerId && h.Deleted == false);

            return row == null ? null : new SchoolExamRateHeaderResultDTO
            {
                Id = row.Id,
                Name = row.Name,
                Percentage = row.Percentage,
                Note = row.Note,
                SchoolId = row.SchoolId
            };
        }



    }
}
