using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Infrastructure.Helpers;
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
    public class EmailProviderCommandService : IBaseCommandService<EmailProvider>, IEmailProviderCommandService
    {
        private readonly IBaseCommandRepository<EmailProvider> commandRepository;
        private readonly IBaseSharedRepository<EmailProvider> sharedRepository;

        public EmailProviderCommandService(IBaseCommandRepository<EmailProvider> commandRepository,
            IBaseSharedRepository<EmailProvider> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }

        public bool Create(EmailProvider entity)
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

        public async Task<bool> DeleteAsync(EmailProvider entity)
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

        public bool Update(EmailProvider entity)
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
