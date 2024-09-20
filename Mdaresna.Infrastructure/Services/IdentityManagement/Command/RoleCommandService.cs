using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.IdentityManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.IdentityManagement.Command
{
    public class RoleCommandService : IBaseCommandService<Role>, IRoleCommandService
    {
        private readonly IBaseCommandRepository<Role> commandRepository;
        private readonly IBaseSharedRepository<Role> sharedRepository;
        private readonly IBaseCommandBulkRepository<RolePermission> permissionsBulkRepository;

        public RoleCommandService(IBaseCommandRepository<Role> commandRepository,
            IBaseSharedRepository<Role> sharedRepository,
            IBaseCommandBulkRepository<RolePermission> permissionsBulkRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
            this.permissionsBulkRepository = permissionsBulkRepository;
        }
        public bool Create(Role entity)
        {
            try
            {
                entity.CreateDate = DateTime.Now;
                entity.LastModifyDate = DateTime.Now;
                entity.Id = DataGenerationHelper.GenerateRowId();
                return commandRepository.Create(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> Create(Role entity, IEnumerable<Guid> permissions)
        {
            try
            {
                entity.CreateDate = DateTime.Now;
                entity.LastModifyDate = DateTime.Now;
                entity.Id = DataGenerationHelper.GenerateRowId();
                var created = commandRepository.Create(entity);
                if(!created)
                    return false;

                var rolePermissions = new List<RolePermission>();

                foreach(var permission in permissions)
                {
                    var rolePermission = new RolePermission
                    {
                        CreateDate = DateTime.Now,
                        LastModifyDate = DateTime.Now,
                        PermissionId = permission,
                        RoleId = entity.Id
                    };

                    rolePermissions.Add(rolePermission);
                }


                return await permissionsBulkRepository.CreateBulk(rolePermissions);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(Role entity)
        {
            try
            {
                entity = await sharedRepository.GetAsync(entity.Id);
                return commandRepository.Delete(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Update(Role entity)
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
