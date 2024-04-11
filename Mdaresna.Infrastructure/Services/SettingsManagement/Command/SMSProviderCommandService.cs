using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Infrastructure.Hilpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.SettingsManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SettingsManagement.Command
{
    public class SMSProviderCommandService: IBaseCommandService<SMSProvider>, ISMSProviderCommandService
    {
        private readonly IBaseCommandRepository<SMSProvider> commandRepository;
        private readonly IBaseSharedRepository<SMSProvider> sharedRepository;

        public SMSProviderCommandService(IBaseCommandRepository<SMSProvider> commandRepository,
            IBaseSharedRepository<SMSProvider> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }

        public bool Create(SMSProvider entity)
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

        public async Task<bool> DeleteAsync(SMSProvider entity)
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

        public bool Update(SMSProvider entity)
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
