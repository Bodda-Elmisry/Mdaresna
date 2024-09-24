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
    public class CoinTypeQueryService : BaseQueryService<CoinType>, ICoinTypeQueryService
    {
        private readonly ICoinTypeQueryRepository coinTypeQueryRepository;

        public CoinTypeQueryService(IBaseQueryRepository<CoinType> queryRepository,
            IBaseSharedRepository<CoinType> sharedRepository,
            ICoinTypeQueryRepository coinTypeQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.coinTypeQueryRepository = coinTypeQueryRepository;
        }

        public async Task<IEnumerable<CoinType>> GetCoinTypesListAsync(string? name, decimal? value, string? notes)
        {
            return await coinTypeQueryRepository.GetCoinTypesListAsync(name, value, notes);
        }
    }
}
