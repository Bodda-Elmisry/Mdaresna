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

            var query = from r in context.Roles
                        join rpGroup in (
                            from rolePermission in context.RolePermissions
                            group rolePermission by rolePermission.RoleId into g
                            select new
                            {
                                RoleID = g.Key,
                                PermissionsCount = g.Count()
                            }
                        ) on r.Id equals rpGroup.RoleID into rps
                        from rp in rps.DefaultIfEmpty()
                        select new
                        {
                            Role = r,
                            PermissionsCount = rp != null ? rp.PermissionsCount : 0
                        };

            query = query.Where(r => r.Role.Deleted == false);

            query = type == 1 ? query.Where(r => r.Role.SchoolRole == true) : query.Where(r => r.Role.AdminRole == true);

            if (type == 1 && schoolId.HasValue)
            {
                query = query.Where(r => r.Role.SchoolId == null || r.Role.SchoolId == schoolId.Value);
            }
            else if (type == 1 && !schoolId.HasValue)
            {
                query = query.Where(r => r.Role.SchoolId == null);
            }

            query = !string.IsNullOrEmpty(name) ? query.Where(r => r.Role.Name.Contains(name)) : query;

            query = !string.IsNullOrEmpty(description) ? query.Where(r => r.Role.Description.Contains(description)) : query;

            query = activation != null ? query.Where(r => r.Role.Active == activation) : query;

            query = ignoredRoles != null ? query.Where(r => !ignoredRoles.Contains(r.Role.Id)) : query;

            return await query.Select(r => new RoleResultDTO
            {
                RoleId = r.Role.Id,
                Name = r.Role.Name,
                Active = r.Role.Active, 
                AdminRole = r.Role.AdminRole,
                Description = r.Role.Description,
                SchoolRole = r.Role.SchoolRole,
                SchoolId = r.Role.SchoolId,
                PermissionsCount = r.PermissionsCount
            }).ToListAsync();

        }





    }
}
