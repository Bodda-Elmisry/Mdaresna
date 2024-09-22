using Mdaresna.Doamin.DTOs.Identity;
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
    public class UserRoleCommandService : IBaseCommandService<UserRole>, IUserRoleCommandService
    {
        private readonly IBaseCommandRepository<UserRole> commandRepository;
        private readonly IBaseSharedRepository<UserRole> sharedRepository;
        private readonly IBaseCommandBulkRepository<UserRole> baseCommandBulkRepository;

        public UserRoleCommandService(IBaseCommandRepository<UserRole> commandRepository,
            IBaseSharedRepository<UserRole> sharedRepository,
            IBaseCommandBulkRepository<UserRole> baseCommandBulkRepository,
            IUserRoleCommandRepository userRoleCommandRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
            this.baseCommandBulkRepository = baseCommandBulkRepository;
        }

        public bool Create(UserRole entity)
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

        public async Task<bool> Create(IEnumerable<UserRoleDTO> entities)
        {
            try
            {
                var userRoles = entities.Select(e => new UserRole
                {
                    RoleId = e.RoleId,
                    SchoolId = e.SchoolId,
                    UserId = e.UserId,
                    CreateDate = DateTime.Now,
                    LastModifyDate = DateTime.Now
                });

                return await baseCommandBulkRepository.CreateBulk(userRoles);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(UserRole entity)
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

        public async Task<bool> DeleteAsync(IEnumerable<UserRoleDTO> entities)
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

        public bool Update(UserRole entity)
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
