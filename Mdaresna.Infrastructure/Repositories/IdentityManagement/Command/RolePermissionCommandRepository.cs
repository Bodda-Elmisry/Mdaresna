using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.Identity.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.Identity.Command
{
    public class RolePermissionCommandRepository : BaseCommandRepository<RolePermission>, IRolePermissionCommandRepository
    {
        public RolePermissionCommandRepository(AppDbContext context) : base(context)
        {
        }
    }
}
