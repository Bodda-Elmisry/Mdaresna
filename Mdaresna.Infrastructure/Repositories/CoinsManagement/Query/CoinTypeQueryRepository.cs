using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.CoinsManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.CoinsManagement.Query
{
    public class CoinTypeQueryRepository : BaseQueryRepository<CoinType>, ICoinTypeQueryRepository
    {
        private readonly AppDbContext context;

        public CoinTypeQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<CoinType>> GetCoinTypesListAsync(string? name, decimal? value, string? notes)
        {
            var quey = context.CoinsTypes.Where(x=> x.Deleted == false);

            quey = !string.IsNullOrEmpty(name) ? quey.Where(x => x.Name.Contains(name)) : quey;

            quey = value != null ? quey.Where(x => x.Value == value) : quey;

            quey = !string.IsNullOrEmpty(notes) ? quey.Where(x => x.Note.Contains(notes)) : quey;

            return await quey.ToListAsync();
        }


    }
}
