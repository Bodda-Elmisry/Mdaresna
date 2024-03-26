using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.Base
{
    public interface IBaseSharedRepository<T>
    {
        Task<T> GetAsync(Guid id);

        Task<bool> IsExistAsync(Guid id);
    }
}
