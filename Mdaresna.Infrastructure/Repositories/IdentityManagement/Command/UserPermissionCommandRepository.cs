using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.IdentityManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Mdaresna.Infrastructure.Repositories.IdentityManagement.Command
{
    public class UserPermissionCommandRepository : BaseCommandRepository<UserPermission>, IUserPermissionCommandRepository
    {
        private readonly AppDbContext context;

        public UserPermissionCommandRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

    }
}
