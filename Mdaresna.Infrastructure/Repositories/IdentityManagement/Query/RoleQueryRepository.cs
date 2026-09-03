using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.IdentityManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mdaresna.Doamin.DTOs.Identity;

namespace Mdaresna.Infrastructure.Repositories.IdentityManagement.Query
{
    public class RoleQueryRepository : BaseQueryRepository<Role>, IRoleQueryRepository
    {
        private readonly AppDbContext context;

        public RoleQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<Role?> GetStanderdRole()
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == "standerd" && r.Deleted == false);
            return role;
        }

        public async Task<Role?> GetRoleByNameAsycn(string name)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower() && r.Deleted == false);
            return role;
        }

        public async Task<IEnumerable<RoleResultDTO>> GetRolesAsync(int type, string? name, bool? activation, string? description, IEnumerable<Guid>? ignoredRoles = null, Guid? schoolId = null)
        {
            var query = context.Roles
                .AsNoTracking()
                .Where(role => role.Deleted == false);

            query = type == 1
                ? query.Where(role => role.SchoolRole == true)
                : query.Where(role => role.AdminRole == true);

            if (type == 1 && schoolId.HasValue)
            {
                query = query.Where(role => role.SchoolId == schoolId.Value);
            }
            else if (type == 1 && !schoolId.HasValue)
            {
                query = query.Where(role => role.SchoolId == null);
            }

            query = !string.IsNullOrEmpty(name) ? query.Where(role => role.Name.Contains(name)) : query;

            query = !string.IsNullOrEmpty(description) ? query.Where(role => role.Description.Contains(description)) : query;

            query = activation != null ? query.Where(role => role.Active == activation) : query;

            query = ignoredRoles != null ? query.Where(role => !ignoredRoles.Contains(role.Id)) : query;

            var resultQuery = query.Select(role => new RoleResultDTO
            {
                RoleId = role.Id,
                Name = role.Name,
                Active = role.Active,
                AdminRole = role.AdminRole,
                Description = role.Description,
                SchoolRole = role.SchoolRole,
                SchoolId = role.SchoolId,
                PermissionsCount = context.RolePermissions.Count(rolePermission => rolePermission.RoleId == role.Id)
            });

            return await resultQuery.ToListAsync();

        }





    }
}
