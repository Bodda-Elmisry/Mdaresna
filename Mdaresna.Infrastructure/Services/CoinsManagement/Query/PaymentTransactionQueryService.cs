using Mdaresna.Doamin.DTOs.CoinsManagement;
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
    public class PaymentTransactionQueryService : BaseQueryService<PaymentTransaction>, IPaymentTransactionQueryService
    {
        private readonly IPaymentTransactionQueryRepository paymentTransactionQueryRepository;

        public PaymentTransactionQueryService(IBaseQueryRepository<PaymentTransaction> queryRepository,
            IBaseSharedRepository<PaymentTransaction> sharedRepository,
            IPaymentTransactionQueryRepository paymentTransactionQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.paymentTransactionQueryRepository = paymentTransactionQueryRepository;
        }

        public async Task<IEnumerable<PaymentTransactionResultDTO>> GetPaymentTransactionsListAsync(DateTime? PaymentDateFrom, DateTime? PaymentDateTO, decimal? Amount, Guid? PaymentTypeId, string? FromId, string? FromName, string? FromType, string? ToId, string? ToName, string? ToType, Guid? CoinTypeId, Guid? SchoolRequestId)
        {
            return await paymentTransactionQueryRepository.GetPaymentTransactionsListAsync(PaymentDateFrom,
                                                                                           PaymentDateTO,  
                                                                                           Amount,
                                                                                           PaymentTypeId,
                                                                                           FromId,
                                                                                           FromName,
                                                                                           FromType,
                                                                                           ToId,
                                                                                           ToName,
                                                                                           ToType,
                                                                                           CoinTypeId,
                                                                                           SchoolRequestId);

        }

        public async Task<PaymentTransactionResultDTO?> GetPaymentTransactionByIdAsync(Guid id)
        {
            return await paymentTransactionQueryRepository.GetPaymentTransactionByIdAsync(id);
        }




    }
}
