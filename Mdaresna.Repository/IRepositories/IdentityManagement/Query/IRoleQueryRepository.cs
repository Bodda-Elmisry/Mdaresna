using Mdaresna.Doamin.DTOs.Identity;
using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.IdentityManagement.Query
{
    public interface IRoleQueryRepository : IBaseQueryRepository<Role>
    {
        Task<Role?> GetStanderdRole();
        Task<Role?> GetRoleByNameAsycn(string name);
        Task<IEnumerable<RoleResultDTO>> GetRolesAsync(int type, string? name, bool? activation, string? description);
    }
}
