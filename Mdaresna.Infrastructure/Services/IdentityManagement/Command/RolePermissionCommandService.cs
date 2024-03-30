using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Infrastructure.Hilpers;
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
    public class RolePermissionCommandService : IBaseCommandService<RolePermission>, IRolePermissionCommandService
    {
        private readonly IBaseCommandRepository<RolePermission> commandRepository;
        private readonly IBaseSharedRepository<RolePermission> sharedRepository;

        public RolePermissionCommandService(IBaseCommandRepository<RolePermission> commandRepository,
            IBaseSharedRepository<RolePermission> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(RolePermission entity)
        {
            try
            {
                return commandRepository.Create(entity);
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

        public bool Update(RolePermission entity)
        {
            try
            {
                return commandRepository.Update(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
