using Mdaresna.Doamin.DTOs.CoinsManagement;
using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.CoinsManagement.Query
{
    public interface IPaymentTransactionQueryRepository : IBaseQueryRepository<PaymentTransaction>
    {
        Task<IEnumerable<PaymentTransactionResultDTO>> GetPaymentTransactionsListAsync(DateTime? PaymentDateFrom,
                                                                                                    DateTime? PaymentDateTO,
                                                                                                    decimal? Amount,
                                                                                                    Guid? PaymentTypeId,
                                                                                                    string? FromId,
                                                                                                    string? FromName,
                                                                                                    string? FromType,
                                                                                                    string? ToId,
                                                                                                    string? ToName,
                                                                                                    string? ToType,
                                                                                                    Guid? CoinTypeId,
                                                                                                    Guid? SchoolRequestId);

        Task<PaymentTransactionResultDTO?> GetPaymentTransactionByIdAsync(Guid id);
    }
}
