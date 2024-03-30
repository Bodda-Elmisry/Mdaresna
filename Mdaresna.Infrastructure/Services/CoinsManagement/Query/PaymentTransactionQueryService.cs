using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
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
        private readonly IBaseQueryRepository<PaymentTransaction> queryRepository;
        private readonly IBaseSharedRepository<PaymentTransaction> sharedRepository;

        public PaymentTransactionQueryService(IBaseQueryRepository<PaymentTransaction> queryRepository,
            IBaseSharedRepository<PaymentTransaction> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
