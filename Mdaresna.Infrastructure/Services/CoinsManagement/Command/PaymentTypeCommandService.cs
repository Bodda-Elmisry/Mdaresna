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
    public class PaymentTypeCommandService : IBaseCommandService<PaymentType>, IPaymentTypeCommandService
    {
        private readonly IBaseCommandRepository<PaymentType> commandRepository;
        private readonly IBaseSharedRepository<PaymentType> sharedRepository;

        public PaymentTypeCommandService(IBaseCommandRepository<PaymentType> commandRepository,
            IBaseSharedRepository<PaymentType> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(PaymentType entity)
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

        public async Task<bool> DeleteAsync(PaymentType entity)
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

        public bool Update(PaymentType entity)
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
