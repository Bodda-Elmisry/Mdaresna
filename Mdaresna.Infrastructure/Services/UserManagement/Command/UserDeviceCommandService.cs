using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Helpers;
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
    public class UserDeviceCommandService : IBaseCommandService<UserDevice>, IUserDeviceCommandService
    {
        private readonly IBaseCommandRepository<UserDevice> commandRepository;
        private readonly IBaseSharedRepository<UserDevice> sharedRepository;

        public UserDeviceCommandService(IBaseCommandRepository<UserDevice> commandRepository,
            IBaseSharedRepository<UserDevice> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }

        public bool Create(UserDevice entity)
        {
            try
            {
                entity.Id = DataGenerationHelper.GenerateRowId();
                entity.CreateDate = DateTime.Now;
                entity.LastModifyDate = DateTime.Now;
                entity.LastSeen = DateTime.Now;
                return commandRepository.Create(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(UserDevice entity)
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

        public bool Update(UserDevice entity)
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
