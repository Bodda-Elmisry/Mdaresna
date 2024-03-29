using Mdaresna.Infrastructure.Data;
using Mdaresna.Repository.IRepositories.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.Base
{
    public class BaseSharedRepository<T> : IBaseSharedRepository<T> where T : class
    {
        private readonly AppDbContext context;

        public BaseSharedRepository(AppDbContext context)
        {
            this.context = context;
        }
        public async Task<bool> IsExistAsync(Guid id)
        {
            var entity =  await context.Set<T>().FindAsync(id);
            return entity != null;
        }
    }
}
