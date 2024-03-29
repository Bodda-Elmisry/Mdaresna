using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.Identity.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.Identity.Query
{
    public class RolePermissionQueryRepository : BaseQueryRepository<RolePermission>, IRolePermissionQueryRepository
    {
       public RolePermissionQueryRepository(AppDbContext context) : base(context)
        {
        }
    }
}
