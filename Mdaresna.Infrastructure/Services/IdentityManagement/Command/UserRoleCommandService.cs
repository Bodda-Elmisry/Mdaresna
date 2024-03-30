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
    public class UserRoleCommandService : IBaseCommandService<UserRole>, IUserRoleCommandService
    {
        private readonly IBaseCommandRepository<UserRole> commandRepository;
        private readonly IBaseSharedRepository<UserRole> sharedRepository;

        public UserRoleCommandService(IBaseCommandRepository<UserRole> commandRepository,
            IBaseSharedRepository<UserRole> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(UserRole entity)
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

        public bool Update(UserRole entity)
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
