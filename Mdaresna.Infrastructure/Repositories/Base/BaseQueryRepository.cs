using Mdaresna.Doamin.Models.Base;
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
    public class BaseQueryRepository<T> : IBaseQueryRepository<T> where T : AuditBase
    {
        private readonly AppDbContext context;

        public BaseQueryRepository(AppDbContext context)
        {
            this.context = context;
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            //return await context.Set<T>().ToListAsync();
            return await context.Set<T>()
            .Where(e => !e.Deleted)
            .ToListAsync();
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            //return await context.Set<T>().FindAsync(id);
            var entity = await context.Set<T>().FindAsync(id);
            return entity is not null && !entity.Deleted ? entity : null;
        }
    }
}
