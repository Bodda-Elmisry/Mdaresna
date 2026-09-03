using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.IdentityManagement.Command;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.IdentityManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.IdentityManagement.Command
{
    public class RolePermissionCommandService : IBaseCommandService<RolePermission>, IRolePermissionCommandService
    {
        private readonly IBaseCommandRepository<RolePermission> commandRepository;
        private readonly IBaseSharedRepository<RolePermission> sharedRepository;
        private readonly IBaseCommandBulkRepository<RolePermission> baseCommandBulkRepository;
        private readonly IRolePermissionCommandRepository rolePermissionCommandRepository;

        public RolePermissionCommandService(IBaseCommandRepository<RolePermission> commandRepository,
            IBaseSharedRepository<RolePermission> sharedRepository,
            IBaseCommandBulkRepository<RolePermission> baseCommandBulkRepository,
            IRolePermissionCommandRepository rolePermissionCommandRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
            this.baseCommandBulkRepository = baseCommandBulkRepository;
            this.rolePermissionCommandRepository = rolePermissionCommandRepository;
        }
        public bool Create(RolePermission entity)
        {
            try
            {
                entity.CreateDate = DateTime.Now;
                entity.LastModifyDate = DateTime.Now;
                return commandRepository.Create(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> Create(IEnumerable<RolePermission> entitiesList)
        {
            try
            {
                return await baseCommandBulkRepository.CreateBulk(entitiesList);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(RolePermission entity)
        {
            try
            {
                return commandRepository.Delete(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(IEnumerable<RolePermission> entitiesList)
        {
            try
            {
                return await baseCommandBulkRepository.DeleteBulk(entitiesList);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Task<bool> ReplaceRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds)
        {
            return rolePermissionCommandRepository.ReplaceRolePermissionsAsync(roleId, permissionIds);
        }

        public bool Update(RolePermission entity)
        {
            try
            {
                entity.LastModifyDate = DateTime.Now;
                return commandRepository.Update(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
