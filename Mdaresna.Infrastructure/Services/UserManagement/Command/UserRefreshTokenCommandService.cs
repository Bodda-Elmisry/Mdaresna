using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.UserManagement.Command;
using System;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.UserManagement.Command
{
    public class UserRefreshTokenCommandService : IBaseCommandService<UserRefreshToken>, IUserRefreshTokenCommandService
    {
        private readonly IBaseCommandRepository<UserRefreshToken> commandRepository;

        public UserRefreshTokenCommandService(IBaseCommandRepository<UserRefreshToken> commandRepository)
        {
            this.commandRepository = commandRepository;
        }

        public bool Create(UserRefreshToken entity)
        {
            try
            {
                entity.Id = DataGenerationHelper.GenerateRowId();
                entity.CreateDate = DateTime.Now;
                entity.LastModifyDate = DateTime.Now;
                entity.IsRevoked = false;
                entity.Deleted = false;
                return commandRepository.Create(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(UserRefreshToken entity)
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

        public bool Update(UserRefreshToken entity)
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
