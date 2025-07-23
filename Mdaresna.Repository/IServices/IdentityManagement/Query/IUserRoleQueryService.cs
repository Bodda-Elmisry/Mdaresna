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
        Task<UserRoleResultDTO?> GetUserRoleViewAsync(Guid userId, Guid roleId, Guid? schoolId);
        Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId, Guid? schoolId);
        Task<IEnumerable<UserRoleResultDTO>> GetUserRolesAsync(Guid userId, Guid? schoolId);
        Task<IEnumerable<UserRole>> GetUserRolesDataAsync(Guid userId, Guid? schoolId);
        Task<IEnumerable<UserRoleResultDTO>> GetRoleUsersAsync(Guid roleId, Guid? schoolId);
        Task<IEnumerable<UserRoleResultDTO>> GetSchoolsAdminsAsync();
        Task<IEnumerable<UserRoleResultDTO>> GetApplicationManagersAsync(string name, string phoneNumber);
    }
}
