using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Hilpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.UserManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.UserManagement.Command
{
    public class UserCommandService : IBaseCommandService<User>, IUserCommandService
    {
        private readonly IBaseCommandRepository<User> commandRepository;
        private readonly IBaseSharedRepository<User> sharedRepository;

        public UserCommandService(IBaseCommandRepository<User> commandRepository,
            IBaseSharedRepository<User> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(User entity)
        {
            try
            {
                entity.Id = DataGenerationHilper.GenerateRowId();
                return commandRepository.Create(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(User entity)
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

        public bool Update(User entity)
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
