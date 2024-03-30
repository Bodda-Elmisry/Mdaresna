using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.Base
{
    public abstract class BaseQueryService<T> : IBaseQueryService<T>
    {
        private readonly IBaseQueryRepository<T> queryRepository;
        private readonly IBaseSharedRepository<T> sharedRepository;

        public BaseQueryService(IBaseQueryRepository<T> queryRepository,
            IBaseSharedRepository<T> sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await queryRepository.GetAllAsync();
        }

        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            return await sharedRepository.GetAsync(id);
        }
    }
}
