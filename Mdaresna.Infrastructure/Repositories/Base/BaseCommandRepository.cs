using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Repository.IRepositories.Base;

namespace Mdaresna.Infrastructure.Repositories.Base
{
    public class BaseCommandRepository<T> : IBaseCommandRepository<T>
    {
        private readonly AppDbContext context;

        public BaseCommandRepository(AppDbContext context)
        {
            this.context = context;
        }
        public bool CreateAsync(T entity)
        {
            context.Add(entity);
            context.SaveChanges();
            return true;
        }

        public bool DeleteAsync(T entity)
        {
            context.Remove(entity);
            context.SaveChanges();
            return true;
        }

        public bool UpdateAsync(T entity)
        {
            context.Update(entity);
            context.SaveChanges();
            return true;
        }
    }
}
