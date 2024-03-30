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
    public class PaymentTypeQueryService : BaseQueryService<PaymentType>, IPaymentTypeQueryService
    {
        private readonly IBaseQueryRepository<PaymentType> queryRepository;
        private readonly IBaseSharedRepository<PaymentType> sharedRepository;

        public PaymentTypeQueryService(IBaseQueryRepository<PaymentType> queryRepository,
            IBaseSharedRepository<PaymentType> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
