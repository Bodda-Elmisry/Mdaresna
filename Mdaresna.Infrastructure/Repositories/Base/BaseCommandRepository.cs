using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Repository.IRepositories.Base;

namespace Mdaresna.Infrastructure.Repositories.Base
{
    public class BaseCommandRepository<T> : IBaseCommandRepository<T> where T : class
    {
        private readonly AppDbContext context;

        public BaseCommandRepository(AppDbContext context)
        {
            this.context = context;
        }

        public IQueryable<T> GetQuery()
        {
            return context.Set<T>().AsQueryable();
        }

        public bool Create(T entity)
        {
            context.Add(entity);
            context.SaveChanges();
            return true;
        }

        public bool Delete(T entity)
        {
            context.Remove(entity);
            context.SaveChanges();
            return true;
        }

        public bool Update(T entity)
        {
            context.Update(entity);
            context.SaveChanges();
            return true;
        }


    }
}
