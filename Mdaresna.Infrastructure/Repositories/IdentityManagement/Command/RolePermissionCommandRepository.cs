using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.IdentityManagement.Command;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.IdentityManagement.Command
{
    public class RolePermissionCommandRepository : BaseCommandRepository<RolePermission>, IRolePermissionCommandRepository
    {
        private readonly AppDbContext context;

        public RolePermissionCommandRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<bool> ReplaceRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds)
        {
            var requestedIds = permissionIds.Distinct().ToHashSet();
            var existing = await context.RolePermissions
                .Where(rolePermission => rolePermission.RoleId == roleId)
                .ToListAsync();

            var toRemove = existing
                .Where(rolePermission => !requestedIds.Contains(rolePermission.PermissionId))
                .ToList();
            var existingIds = existing
                .Select(rolePermission => rolePermission.PermissionId)
                .ToHashSet();
            var now = DateTime.UtcNow;
            var toAdd = requestedIds
                .Where(permissionId => !existingIds.Contains(permissionId))
                .Select(permissionId => new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    CreateDate = now,
                    LastModifyDate = now
                })
                .ToList();

            await using var transaction = await context.Database.BeginTransactionAsync();
            context.RolePermissions.RemoveRange(toRemove);
            await context.RolePermissions.AddRangeAsync(toAdd);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
    }
}
