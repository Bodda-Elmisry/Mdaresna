using EFCore.BulkExtensions;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.Base
{
    public class BaseCommandBulkRepository<T> : IBaseCommandBulkRepository<T> where T : class
    {
        private readonly AppDbContext context;

        public BaseCommandBulkRepository(AppDbContext context)
        {
            this.context = context;
        }

        public IQueryable<T> GetQuery()
        {
            return context.Set<T>().AsQueryable();
        }

        public async Task<bool> CreateBulk<T>(IEnumerable<T> entityList) where T : class
        {
            await context.BulkInsertAsync(entityList, BulkConfig =>
            {
                BulkConfig.IncludeGraph = true;
            });
            return true;
        }

        public async Task<bool> DeleteBulk<T>(IEnumerable<T> entityList) where T : class
        {
            await context.BulkDeleteAsync(entityList);
            return true;
        }
    }
}
