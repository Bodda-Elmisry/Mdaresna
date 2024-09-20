using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.IdentityManagement.Command
{
    public interface IRolePermissionCommandService : IBaseCommandService<RolePermission>
    {
        Task<bool> Create(IEnumerable<RolePermission> entitiesList);
        Task<bool> DeleteAsync(IEnumerable<RolePermission> entitiesList);
    }
}
