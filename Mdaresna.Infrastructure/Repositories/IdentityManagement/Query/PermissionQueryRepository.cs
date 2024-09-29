using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.IdentityManagement.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.IdentityManagement.Query
{
    public class PermissionQueryRepository : BaseQueryRepository<Permission>, IPermissionQueryRepository
    {
        private readonly AppDbContext _context;
        private readonly AppSettingDTO appSettings;

        public PermissionQueryRepository(AppDbContext context,
                                                IOptions<AppSettingDTO> appSettings) : base(context)
        {
            this._context = context;
            this.appSettings = appSettings.Value;
        }

        public async Task<List<Permission>> GetPermissionsListAsync(int permissionsType, int pageNumber, string? permissionName)
        {

            var result = new List<Permission>();


            switch (permissionsType)
            {
                case 1:
                    result = await GetSchoolPermissions(pageNumber, permissionName);
                    break;
                case 2:
                    result = await GetAppPermissions(pageNumber, permissionName);
                    break;
            }

            return result;
        }

        private async Task<List<Permission>> GetSchoolPermissions(int pageNumber, string? permissionName)
        {
            int pagesize = this.appSettings.PageSize != null ? this.appSettings.PageSize.Value : 30;
            var query = _context.Permissions.Where(p => p.SchoolPermission == true);
            query = !string.IsNullOrEmpty(permissionName) ? query.Where(p=> p.Name.Contains(permissionName)) : query;
            return await query.OrderBy(p => p.Name)
                                  .Skip((pageNumber - 1) * pagesize)
                                  .Take(pagesize).ToListAsync();

        }

        private async Task<List<Permission>> GetAppPermissions(int pageNumber, string? permissionName)
        {
            int pagesize = this.appSettings.PageSize != null ? this.appSettings.PageSize.Value : 30;
            var query = _context.Permissions.Where(p => p.AppPermission == true);
            query = !string.IsNullOrEmpty(permissionName) ? query.Where(p => p.Name.Contains(permissionName)) : query;
            return await query.OrderBy(p => p.Name)
                                  .Skip((pageNumber - 1) * pagesize)
                                  .Take(pagesize).ToListAsync();
        }








    }
}
