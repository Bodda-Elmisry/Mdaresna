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
    public class UserRoleQueryRepository : BaseQueryRepository<UserRole>, IUserRoleQueryRepository
    {
        private readonly AppDbContext context;

        public UserRoleQueryRepository(AppDbContext context) 
            : base(context)
        {
            this.context = context;
        }

        public async Task<bool> CheckUserRole(UserRole userRole)
        {
            return await context.UserRoles.AnyAsync(u => u.RoleId == userRole.RoleId && u.UserId == userRole.UserId);

        }
    }
}
