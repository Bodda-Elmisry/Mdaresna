using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.SettingsManagement.Command;
using System;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SettingsManagement.Command
{
    public class SMSLogCommandService : IBaseCommandService<SMSLog>, ISMSLogCommandService
    {
        private readonly IBaseCommandRepository<SMSLog> commandRepository;
        private readonly IBaseSharedRepository<SMSLog> sharedRepository;

        public SMSLogCommandService(IBaseCommandRepository<SMSLog> commandRepository,
            IBaseSharedRepository<SMSLog> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }

        public bool Create(SMSLog entity)
        {
            try
            {
                entity.Id = DataGenerationHelper.GenerateRowId();
                entity.CreateDate = DateTime.Now;
                entity.LastModifyDate = DateTime.Now;
                return commandRepository.Create(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(SMSLog entity)
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

        public bool Update(SMSLog entity)
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
