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
    public class SchoolPaymentRequestQueryService : BaseQueryService<SchoolPaymentRequest>, ISchoolPaymentRequestQueryService
    {
        private readonly ISchoolPaymentRequestQueryRepository schoolPaymentRequestQueryRepository;

        public SchoolPaymentRequestQueryService(IBaseQueryRepository<SchoolPaymentRequest> queryRepository,
            IBaseSharedRepository<SchoolPaymentRequest> sharedRepository,
            ISchoolPaymentRequestQueryRepository schoolPaymentRequestQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.schoolPaymentRequestQueryRepository = schoolPaymentRequestQueryRepository;
        }

        public async Task<IEnumerable<SchoolPaymentRequestResultDTO>> GetSchoolPaymentRequestsListAsync(DateTime? requestDateFrom, DateTime? requestDateTo, string? transfareCode, DateTime? transfareDateFrom, DateTime? transfareDateTo, decimal? transfareAmount, Guid? paymentTypeId, Guid? schoolId, bool? approvied, Guid? approviedById)
        {
            return await schoolPaymentRequestQueryRepository.GetSchoolPaymentRequestsListAsync(requestDateFrom,
                                                                                               requestDateTo,
                                                                                               transfareCode,
                                                                                               transfareDateFrom,
                                                                                               transfareDateTo,
                                                                                               transfareAmount,
                                                                                               paymentTypeId,
                                                                                               schoolId,
                                                                                               approvied,
                                                                                               approviedById);
        }

        public async Task<SchoolPaymentRequestResultDTO?> GetSchoolPaymentRequestViewAsync(Guid id)
        {
            return await schoolPaymentRequestQueryRepository.GetSchoolPaymentRequestViewAsync(id);
        }
    }
}
