using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.CoinsManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.CoinsManagement.Query
{
    public class PaymentTypeQueryRepository : BaseQueryRepository<PaymentType>, IPaymentTypeQueryRepository
    {
        private readonly AppDbContext context;

        public PaymentTypeQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<PaymentType>> GetPaumentTypesAsync(string? name, string? notes, bool? isActive)
        {
            var query = context.PaymentTypes.AsQueryable();

            query = !string.IsNullOrEmpty(name) ? query.Where(t=> t.Name.Contains(name)) : query;

            query = !string.IsNullOrEmpty(notes) ? query.Where(t => t.Note.Contains(notes)) : query;

            query = isActive != null ? query.Where(t => t.IsActive == isActive) : query;

            return await query.OrderBy(t => t.Name).ToListAsync();
        }

        public async Task<PaymentType?> GetPaumentTypeByNameAsync(string name)
        {
            return await context.PaymentTypes.FirstOrDefaultAsync(t => t.Name == name);
        }










    }
}
