using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.CoinsManagement.Query
{
    public interface ICoinTypeQueryRepository : IBaseQueryRepository<CoinType>
    {
        Task<IEnumerable<CoinType>> GetCoinTypesListAsync(string? name, decimal? value, string? notes);
    }
}
