using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.CoinsManagement.Query
{
    public interface ICoinTypeQueryService : IBaseQueryService<CoinType>
    {
        Task<IEnumerable<CoinType>> GetCoinTypesListAsync(string? name, decimal? value, string? notes);
    }
}
