using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.UserManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.UserManagement.Query
{
    public class RelationTypeQueryRepository : BaseQueryRepository<RelationType>, IRelationTypeQueryRepository
    {
        private readonly AppDbContext context;

        public RelationTypeQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<RelationType?> GetRelationByNameAsync(string name)
        {
            return await context.relationTypes.FirstOrDefaultAsync(t => t.Name == name && t.Deleted == false);
        }


    }
}
