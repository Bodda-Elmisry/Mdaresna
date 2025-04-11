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
    public interface IRoleQueryService : IBaseQueryService<Role>
    {
        Task<Role?> GetStanderdRole();
        Task<Role?> GetRoleByNameAsycn(string name);
        Task<IEnumerable<RoleResultDTO>> GetRolesAsync(int type, string? name, bool? activation, string? description, IEnumerable<Guid>? ignoredRoles = null);
    }
}
