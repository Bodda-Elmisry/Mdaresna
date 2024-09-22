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
    public interface IUserRoleQueryService : IBaseQueryService<UserRole>
    {
        Task<bool> CheckRoleExist(UserRole role);
        Task<UserRoleResultDTO?> GetUserRoleAsync(Guid userId, Guid roleId);
        Task<IEnumerable<UserRoleResultDTO>> GetUserRolesAsync(Guid userId, Guid? schoolId);
    }
}
