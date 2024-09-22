using Mdaresna.Doamin.DTOs.Identity;
using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.IdentityManagement.Command
{
    public interface IUserRoleCommandService : IBaseCommandService<UserRole>
    {
        Task<bool> Create(IEnumerable<UserRoleDTO> entities);
        Task<bool> DeleteAsync(IEnumerable<UserRoleDTO> entities);
    }
}
