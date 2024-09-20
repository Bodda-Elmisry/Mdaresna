using Mdaresna.Doamin.DTOs.Identity;
using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.IdentityManagement.Query
{
    public interface IRolePermissionQueryService : IBaseQueryService<RolePermission>
    {
        Task<IEnumerable<RolePermissionResultDTO>> GetRolePermissions(Guid roleId);
    }
}
