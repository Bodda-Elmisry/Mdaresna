using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.Base
{
    public interface IBaseCommandService<T>
    {
        bool Create(T entity);
        bool Update(T entity);
        Task<bool> DeleteAsync(T entity);
    }
}
