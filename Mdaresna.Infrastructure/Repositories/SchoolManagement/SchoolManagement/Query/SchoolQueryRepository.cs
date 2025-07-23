using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Helpers;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
        private readonly AppSettingDTO appSettings;

        public SchoolQueryRepository(AppDbContext context,
                                                IOptions<AppSettingDTO> appSettings) : base(context)
        {
            this.context = context;
            this.appSettings = appSettings.Value;
        }

        public async Task<IEnumerable<SchoolResultDTO>> GetUserAdminSchools(Guid userId)
        {
            return await GetSchoolQuery().Where(s => s.SchoolAdminId == userId).OrderBy(s => s.SchoolTypeId).OrderBy(s => s.Name).ToListAsync();
        }

        public async Task<IEnumerable<SchoolResultDTO>> GetUserSchools(Guid userId, bool? active)
        {

            var tquery = from school in context.Schools
                         join st in context.schoolTeachers on school.Id equals st.SchoolId into stGroup
                         from st in stGroup.DefaultIfEmpty()
                         join schoolType in context.SchoolTypes on school.SchoolTypeId equals schoolType.Id into stTypeGroup
                         from schoolType in stTypeGroup.DefaultIfEmpty()
                         join coinType in context.CoinsTypes on school.CoinTypeId equals coinType.Id into ctGroup
                         from coinType in ctGroup.DefaultIfEmpty()
                         join admin in context.Users on school.SchoolAdminId equals admin.Id into adminGroup
                         from admin in adminGroup.DefaultIfEmpty()
                         where school.Deleted == false && (school.SchoolAdminId == userId || st.TeacherId == userId)

                         select new SchoolResultDTO
                         {
                             Id = school.Id,
                             Name = school.Name,
                             About = school.About,
                             Vesion = school.Vesion,
                             ImageUrl = school.ImageUrl,
                             Active = school.Active,
                             SchoolTypeId = school.SchoolTypeId,
                             SchoolTypeName = schoolType != null ? schoolType.Name : null,
                             CoinTypeId = school.CoinTypeId,
                             CoinTypeName = coinType != null ? coinType.Name : null,
                             AvailableCoins = school.AvailableCoins,
                             SchoolAdminId = school.SchoolAdminId,
                             SchoolAdminName = admin != null ? admin.FirstName + " " + admin.LastName : null
                         };

            var equery = from school in context.Schools
                         join se in context.SchoolEmployees on school.Id equals se.SchoolId into schoolEmployees
                         from se in schoolEmployees.DefaultIfEmpty()
                         join schoolType in context.SchoolTypes on school.SchoolTypeId equals schoolType.Id into stTypeGroup
                         from schoolType in stTypeGroup.DefaultIfEmpty()
                         join coinType in context.CoinsTypes on school.CoinTypeId equals coinType.Id into ctGroup
                         from coinType in ctGroup.DefaultIfEmpty()
                         join admin in context.Users on school.SchoolAdminId equals admin.Id into adminGroup
                         from admin in adminGroup.DefaultIfEmpty()
                         where school.Deleted == false && (school.SchoolAdminId == userId || se.EmployeeId == userId)
                         select new SchoolResultDTO
                         {
                             Id = school.Id,
                             Name = school.Name,
                             About = school.About,
                             Vesion = school.Vesion,
                             ImageUrl = school.ImageUrl,
                             Active = school.Active,
                             SchoolTypeId = school.SchoolTypeId,
                             SchoolTypeName = schoolType != null ? schoolType.Name : null,
                             CoinTypeId = school.CoinTypeId,
                             CoinTypeName = coinType != null ? coinType.Name : null,
                             AvailableCoins = school.AvailableCoins,
                             SchoolAdminId = school.SchoolAdminId,
                             SchoolAdminName = admin != null ? admin.FirstName + " " + admin.LastName : null
                         };

            // Combine the two queries using Union
            var query = tquery.Union(equery).Distinct();

            query = active != null ? query.Where(s => s.Active == active) : query;
            
            Console.WriteLine(query.ToQueryString()); // For debugging
            // Optional: Sort
            var result = await query
                .OrderBy(s => s.SchoolTypeId)
                .ThenBy(s => s.Name)
                .ToListAsync();

            return result;
        }

        public async Task<SchoolResultDTO?> GetSchoolById(Guid schoolId)
        {
            return await GetSchoolQuery().FirstOrDefaultAsync(s => s.Id == schoolId);
        }

        public async Task<IEnumerable<SchoolResultDTO>> GetSchoolsList(string? name, bool? active, Guid? schoolTypeId, Guid? coinTypeId, Guid? adminId, int pageNumber, bool? getNewSchools)
        {
            int pagesize = appSettings.PageSize != null ? appSettings.PageSize.Value : 30;

            var query = GetSchoolQuery();
            if (!string.IsNullOrEmpty(name))
                query = query.Where(s => s.Name.Contains(name));

            if (active != null)
                query = query.Where(s => s.Active == active);

            if (schoolTypeId != null)
                query = query.Where(s => s.SchoolTypeId == schoolTypeId);

            if (coinTypeId != null)
                query = query.Where(s => s.CoinTypeId == coinTypeId);

            if (adminId != null)
                query = query.Where(s => s.SchoolAdminId == adminId);

            if (getNewSchools != null && getNewSchools.Value)
                query = query.Where(s => s.CoinTypeId == null);

            var queryString = query.ToQueryString();

            return await query.OrderBy(s => s.SchoolTypeId).OrderBy(s => s.Name)
                                   .Skip((pageNumber - 1) * pagesize)
                                   .ToListAsync();
        }

        public async Task<IEnumerable<Guid>> GetSchoolUsersIds(Guid schoolId)
        {
            var employeesIdsQuery = context.SchoolEmployees.AsNoTracking()
                                    .Where(e => e.SchoolId == schoolId && e.Deleted == false)
                                    .Select(e => e.EmployeeId);

            var teachersIdsQuery = context.schoolTeachers.AsNoTracking()
                                   .Where(t=> t.SchoolId == schoolId && t.Deleted == false)
                                   .Select(t=> t.TeacherId);

            var query = employeesIdsQuery.Union(teachersIdsQuery);

            return await query.ToListAsync();
        }

        private IQueryable<SchoolResultDTO> GetSchoolQuery()
        {
            var query = context.Schools
                       .Include(s => s.SchoolType).Include(s => s.SchoolAdmin).Include(s => s.CoinType)
                       .Where(s=> s.Deleted == false)
                       .Select(s => new SchoolResultDTO
                       {
                           Id = s.Id,
                           Name = s.Name,
                           About = s.About,
                           Vesion = s.Vesion,
                           Active = s.Active,
                           ImageUrl = !string.IsNullOrEmpty(s.ImageUrl) ? $"{SettingsHelper.GetAppUrl()}/{s.ImageUrl.Replace("\\", "/")}" : string.Empty,
                           SchoolTypeId = s.SchoolTypeId,
                           SchoolTypeName = s.SchoolType.Name,
                           CoinTypeId = s.CoinTypeId,
                           CoinTypeName = s.CoinType.Name,
                           AvailableCoins = s.AvailableCoins,
                           SchoolAdminId = s.SchoolAdminId,
                           SchoolAdminName = $"{s.SchoolAdmin.FirstName} {s.SchoolAdmin.LastName}"
                       });

            return query;
        }



    }
}
