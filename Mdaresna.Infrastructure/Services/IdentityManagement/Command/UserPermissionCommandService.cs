using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Infrastructure.Repositories.Base;
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
    public class UserPermissionCommandService : IBaseCommandService<UserPermission>, IUserPermissionCommandService
    {
        private readonly IBaseCommandRepository<UserPermission> commandRepository;
        private readonly IBaseSharedRepository<UserPermission> sharedRepository;
        private readonly IBaseCommandBulkRepository<UserPermission> baseCommandBulkRepository;

        public UserPermissionCommandService(IBaseCommandRepository<UserPermission> commandRepository,
            IBaseSharedRepository<UserPermission> sharedRepository,
            IBaseCommandBulkRepository<UserPermission> baseCommandBulkRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
            this.baseCommandBulkRepository = baseCommandBulkRepository;
        }
        public bool Create(UserPermission entity)
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
        public async Task<bool> Create(IEnumerable<UserPermission> entities)
        {
            try
            {
                return await baseCommandBulkRepository.CreateBulk(entities);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(UserPermission entity)
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

        public async Task<bool> DeleteAsync(IEnumerable<UserPermission> entities)
        {
            try
            {
                return await baseCommandBulkRepository.DeleteBulk(entities);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Update(UserPermission entity)
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
