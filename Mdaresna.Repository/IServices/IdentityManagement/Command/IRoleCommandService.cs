using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.IdentityManagement.Command
{
    public interface IRoleCommandService : IBaseCommandService<Role>
    {
        Task<bool> Create(Role entity, IEnumerable<Guid> permissions);
    }
}
