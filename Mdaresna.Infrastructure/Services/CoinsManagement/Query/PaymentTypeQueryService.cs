using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.CoinsManagement.Query;
using Mdaresna.Repository.IServices.CoinsManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.CoinsManagement.Query
{
    public class PaymentTypeQueryService : BaseQueryService<PaymentType>, IPaymentTypeQueryService
    {
        private readonly IPaymentTypeQueryRepository paymentTypeQueryRepository;

        public PaymentTypeQueryService(IBaseQueryRepository<PaymentType> queryRepository,
            IBaseSharedRepository<PaymentType> sharedRepository,
            IPaymentTypeQueryRepository paymentTypeQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.paymentTypeQueryRepository = paymentTypeQueryRepository;
        }

        public async Task<PaymentType?> GetPaumentTypeByNameAsync(string name)
        {
            return await paymentTypeQueryRepository.GetPaumentTypeByNameAsync(name);
        }

        public async Task<IEnumerable<PaymentType>> GetPaumentTypesAsync(string? name, string? notes, bool? isActive)
        {
            return await paymentTypeQueryRepository.GetPaumentTypesAsync(name, notes, isActive);
        }


    }
}
