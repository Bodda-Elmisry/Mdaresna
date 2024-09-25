using Mdaresna.Doamin.DTOs.CoinsManagement;
using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.CoinsManagement.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.CoinsManagement.Query
{
    public class PaymentTransactionQueryRepository : BaseQueryRepository<PaymentTransaction>, IPaymentTransactionQueryRepository
    {
        private readonly AppDbContext context;

        public PaymentTransactionQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<PaymentTransactionResultDTO>> GetPaymentTransactionsListAsync(DateTime? PaymentDateFrom, 
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
                                                                                                    Guid? SchoolRequestId)
        {
            var query = context.PaymentTransactions.AsQueryable();

            PaymentDateFrom = PaymentDateFrom != null ? new DateTime(PaymentDateFrom.Value.Year, PaymentDateFrom.Value.Month, PaymentDateFrom.Value.Day) : null;
            PaymentDateTO = PaymentDateTO != null ? new DateTime(PaymentDateTO.Value.Year, PaymentDateTO.Value.Month, PaymentDateTO.Value.Day) : null;

            if (PaymentDateFrom != null && PaymentDateTO != null)
                query = query.Where(t => t.PaymentDate >= PaymentDateFrom && t.PaymentDate <= PaymentDateTO);
            else if(PaymentDateFrom != null)
                query = query.Where(t => t.PaymentDate == PaymentDateFrom);

            query = Amount != null ? query.Where(t=> t.Amount == Amount) : query;

            query = PaymentTypeId != null ? query.Where(t=> t.PaymentTypeId == PaymentTypeId) : query;

            query = !string.IsNullOrEmpty(FromId) ? query.Where(t=> t.FromId.Contains(FromId)) : query;

            query = !string.IsNullOrEmpty(FromName) ? query.Where(t=> t.FromId.Contains(FromName)) : query;
            
            query = !string.IsNullOrEmpty(FromType) ? query.Where(t=> t.FromId.Contains(FromType)) : query;
            
            query = !string.IsNullOrEmpty(ToId) ? query.Where(t=> t.ToId.Contains(ToId)) : query;
            
            query = !string.IsNullOrEmpty(ToName) ? query.Where(t=> t.ToId.Contains(ToName)) : query;
            
            query = !string.IsNullOrEmpty(ToType) ? query.Where(t=> t.ToId.Contains(ToType)) : query;
            
            query = CoinTypeId != null ? query.Where(t => t.CoinTypeId == CoinTypeId) : query;
            
            query = SchoolRequestId != null ? query.Where(t => t.SchoolRequestId == SchoolRequestId) : query;

            query = query.Include(t => t.paymentType)
                         .Include(t => t.CoinType)
                         .Include(t => t.SchoolPaymentRequest)
                         .OrderByDescending(t=> t.PaymentDate);

            return await query.Select(t => new PaymentTransactionResultDTO
            {
                Id = t.Id,
                PaymentDate = t.PaymentDate,
                Amount = t.Amount,
                PaymentTypeId = t.PaymentTypeId,
                paymentType = t.paymentType.Name,
                FromId = t.FromId,
                ToId = t.ToId,
                CoinTypeId = t.CoinTypeId,
                CoinType = t.CoinType.Name,
                SchoolRequestId = t.SchoolRequestId,
                SchoolPaymentRequest = t.SchoolPaymentRequest != null ? t.SchoolPaymentRequest.TransfareCode : string.Empty
            }).ToListAsync();


        }


        public async Task<PaymentTransactionResultDTO?> GetPaymentTransactionByIdAsync(Guid id)
        {
            var transaction = await context.PaymentTransactions.FirstOrDefaultAsync(t => t.Id == id);

            return transaction == null ? null : new PaymentTransactionResultDTO
            {
                Id = transaction.Id,
                PaymentDate = transaction.PaymentDate,
                Amount = transaction.Amount,
                PaymentTypeId = transaction.PaymentTypeId,
                paymentType = transaction.paymentType.Name,
                FromId = transaction.FromId,
                ToId = transaction.ToId,
                CoinTypeId = transaction.CoinTypeId,
                CoinType = transaction.CoinType.Name,
                SchoolRequestId = transaction.SchoolRequestId,
                SchoolPaymentRequest = transaction.SchoolPaymentRequest != null ? transaction.SchoolPaymentRequest.TransfareCode : string.Empty
            };
        }


    }
}
