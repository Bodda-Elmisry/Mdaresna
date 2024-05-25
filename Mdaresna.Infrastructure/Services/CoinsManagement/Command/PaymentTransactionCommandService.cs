using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.CoinsManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.CoinsManagement.Command
{
    public class PaymentTransactionCommandService : IBaseCommandService<PaymentTransaction>, IPaymentTransactionCommandService
    {
        private readonly IBaseCommandRepository<PaymentTransaction> commandRepository;
        private readonly IBaseSharedRepository<PaymentTransaction> sharedRepository;

        public PaymentTransactionCommandService(IBaseCommandRepository<PaymentTransaction> commandRepository,
            IBaseSharedRepository<PaymentTransaction> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(PaymentTransaction entity)
        {
            try
            {
                entity.CreateDate = DateTime.Now;
                entity.LastModifyDate = DateTime.Now;
                entity.Id = DataGenerationHelper.GenerateRowId();
                return commandRepository.Create(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(PaymentTransaction entity)
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

        public bool Update(PaymentTransaction entity)
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
