using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.IdentityManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mdaresna.Doamin.DTOs.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Infrastructure.Repositories.IdentityManagement.Query
{
    public class RolePermissionQueryRepository : BaseQueryRepository<RolePermission>, IRolePermissionQueryRepository
    {
        private readonly AppDbContext context;

        public RolePermissionQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<RolePermissionResultDTO>> GetRolePermissions(Guid roleId)
        {
            return await context.RolePermissions.Include(r => r.Role).Include(r => r.Permission)
                                    .Select(r => new RolePermissionResultDTO
                                    {
                                        RoleId = r.RoleId,
                                        RoleName = r.Role.Name,
                                        PermissionId = r.PermissionId,
                                        PermissionName = r.Permission.Name,
                                        PermissionDescription = r.Permission.Description
                                    })
                                    .Where(r=> r.RoleId == roleId)
                                    .ToListAsync();
        }








    }
}
