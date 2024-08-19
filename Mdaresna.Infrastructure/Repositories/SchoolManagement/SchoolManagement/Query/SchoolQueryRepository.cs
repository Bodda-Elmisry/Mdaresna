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
    public class SchoolQueryRepository : BaseQueryRepository<School>, ISchoolQueryRepository
    {
        private readonly AppDbContext context;

        public SchoolQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<SchoolResultDTO>> GetUserAdminSchools(Guid userId)
        {
            return await GetSchoolQuery().Where(s => s.SchoolAdminId == userId).ToListAsync();
        }

        public async Task<SchoolResultDTO?> GetSchoolById(Guid schoolId)
        {
            return await GetSchoolQuery().FirstOrDefaultAsync(s => s.Id == schoolId);
        }

        public async Task<IEnumerable<SchoolResultDTO>> GetSchoolsList()
        {
            return await GetSchoolQuery().ToListAsync();
        }

        private IQueryable<SchoolResultDTO> GetSchoolQuery()
        {
            var query = context.Schools
                       .Include(s => s.SchoolType).Include(s => s.SchoolAdmin).Include(s => s.CoinType)
                       .Select(s => new SchoolResultDTO
                       {
                           Id = s.Id,
                           Name = s.Name,
                           About = s.About,
                           Vesion = s.Vesion,
                           Active = s.Active,
                           ImageUrl = s.ImageUrl,
                           SchoolTypeId = s.SchoolTypeId,
                           SchoolTypeName = s.SchoolType.Name,
                           CoinTypeId = s.CoinTypeId,
                           CoinTypeName = s.CoinType.Name,
                           AvailableCoins = s.AvailableCoins,
                           SchoolAdminId = s.SchoolAdminId,
                           SchoolAdminName = s.SchoolType.Name
                       });

            return query;
        }
    }
}
