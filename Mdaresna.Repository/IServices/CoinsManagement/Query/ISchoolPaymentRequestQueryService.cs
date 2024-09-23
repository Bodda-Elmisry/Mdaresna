using Mdaresna.Doamin.DTOs.CoinsManagement;
using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.CoinsManagement.Query
{
    public interface ISchoolPaymentRequestQueryService : IBaseQueryService<SchoolPaymentRequest>
    {
        Task<IEnumerable<SchoolPaymentRequestResultDTO>> GetSchoolPaymentRequestsListAsync(DateTime? requestDateFrom, DateTime? requestDateTo, string? transfareCode, DateTime? transfareDateFrom, DateTime? transfareDateTo,
                                                                                                    decimal? transfareAmount, Guid? paymentTypeId, Guid? schoolId, bool? approvied, Guid? approviedById);

        Task<SchoolPaymentRequestResultDTO?> GetSchoolPaymentRequestViewAsync(Guid id);
    }
}
