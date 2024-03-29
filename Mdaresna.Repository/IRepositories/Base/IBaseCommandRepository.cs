using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.Base
{
    public interface IBaseCommandRepository<T>
    {
        bool CreateAsync(T entity);
        bool UpdateAsync(T entity);
        bool DeleteAsync(T entity);
    }
}
