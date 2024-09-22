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
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == "standerd");
            return role;
        }

        public async Task<Role?> GetRoleByNameAsycn(string name)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower());
            return role;
        }

        public async Task<IEnumerable<RoleResultDTO>> GetRolesAsync(int type, string? name, bool? activation, string? description)
        {

            var query = from r in context.Roles
                        join rp in (
                            from rolePermission in context.RolePermissions
                            group rolePermission by rolePermission.RoleId into g
                            select new
                            {
                                RoleID = g.Key,
                                PermissionsCount = g.Count()
                            }
                        ) on r.Id equals rp.RoleID
                        select new
                        {
                            Role = r,
                            PermissionsCount = rp.PermissionsCount
                        };


            query = type == 1 ? query.Where(r => r.Role.SchoolRole == true) : query.Where(r => r.Role.AdminRole == true);

            //query = type == 1 ? query.Where(r => r.SchoolRole == true) : query.Where(r => r.AdminRole == true);

            query = !string.IsNullOrEmpty(name) ? query.Where(r => r.Role.Name.Contains(name)) : query;

            query = !string.IsNullOrEmpty(description) ? query.Where(r => r.Role.Description.Contains(description)) : query;

            query = activation != null ? query.Where(r => r.Role.Active == activation) : query;

            return await query.Select(r => new RoleResultDTO
            {
                RoleId = r.Role.Id,
                Name = r.Role.Name,
                Active = r.Role.Active, 
                AdminRole = r.Role.AdminRole,
                Description = r.Role.Description,
                SchoolRole = r.Role.SchoolRole,
                PermissionsCount = r.PermissionsCount
            }).ToListAsync();

        }





    }
}
