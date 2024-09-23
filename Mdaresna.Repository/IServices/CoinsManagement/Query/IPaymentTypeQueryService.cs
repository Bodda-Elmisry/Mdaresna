using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.CoinsManagement.Query
{
    public interface IPaymentTypeQueryService : IBaseQueryService<PaymentType>
    {
        Task<IEnumerable<PaymentType>> GetPaumentTypesAsync(string? name, string? notes, bool? isActive);
        Task<PaymentType?> GetPaumentTypeByNameAsync(string name);
    }
}
