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
    public class SchoolPaymentRequestQueryService : BaseQueryService<SchoolPaymentRequest>, ISchoolPaymentRequestQueryService
    {
        private readonly IBaseQueryRepository<SchoolPaymentRequest> queryRepository;
        private readonly IBaseSharedRepository<SchoolPaymentRequest> sharedRepository;

        public SchoolPaymentRequestQueryService(IBaseQueryRepository<SchoolPaymentRequest> queryRepository,
            IBaseSharedRepository<SchoolPaymentRequest> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
