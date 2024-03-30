using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
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
        private readonly IBaseQueryRepository<CoinType> queryRepository;
        private readonly IBaseSharedRepository<CoinType> sharedRepository;

        public CoinTypeQueryService(IBaseQueryRepository<CoinType> queryRepository,
            IBaseSharedRepository<CoinType> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
