using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.IdentityManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.IdentityManagement.Command
{
    public class PermissionCommandRepository : BaseCommandRepository<Permission>, IPermissionCommandRepository
    {
        public PermissionCommandRepository(AppDbContext context) : base(context)
        {
        }
    }
}
