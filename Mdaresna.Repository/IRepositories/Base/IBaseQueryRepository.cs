using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.Base
{
    public interface IBaseQueryRepository<T>
    {
        IQueryable<T> GetQuery();

        Task<IEnumerable<T>> GetAllAsync();

        Task<T> GetByIdAsync(Guid id);


    }
}
