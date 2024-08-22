using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.Base
{
    public interface IBaseCommandBulkRepository<T>
    {
        Task<bool> CreateBulk<T>(IEnumerable<T> entityList) where T : class;
    }
}
