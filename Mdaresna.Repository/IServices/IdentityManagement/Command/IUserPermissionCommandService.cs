using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.IdentityManagement.Command
{
    public interface IUserPermissionCommandService : IBaseCommandService<UserPermission>
    {
        Task<bool> Create(IEnumerable<UserPermission> entities);
        Task<bool> DeleteAsync(IEnumerable<UserPermission> entities);
    }
}
