using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.Base
{
    public interface IBaseCommandRepository<T>
    {
        bool Create(T entity);
        bool Update(T entity);
        bool Delete(T entity);
    }
}
