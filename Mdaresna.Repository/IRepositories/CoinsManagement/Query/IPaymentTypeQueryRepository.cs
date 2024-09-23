using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.CoinsManagement.Query
{
    public interface IPaymentTypeQueryRepository : IBaseQueryRepository<PaymentType>
    {
        Task<IEnumerable<PaymentType>> GetPaumentTypesAsync(string? name, string? notes, bool? isActive);
        Task<PaymentType?> GetPaumentTypeByNameAsync(string name);
    }
}
