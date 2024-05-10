using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.IdentityManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.IdentityManagement.Query
{
    public class RoleQueryRepository : BaseQueryRepository<Role>, IRoleQueryRepository
    {
        private readonly AppDbContext context;

        public RoleQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<Role?> GetStanderdRole()
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == "standerd");
            return role;
        }
    }
}
