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
    public interface IUserRoleQueryRepository : IBaseQueryRepository<UserRole>
    {
        Task<bool> CheckUserRole(UserRole userRole);
        Task<UserRoleResultDTO?> GetUserRoleViewAsync(Guid userId, Guid roleId, Guid? schoolId);
        Task<UserRole> GetUserRoleAsync(Guid userId, Guid roleId, Guid? schoolId);
        Task<IEnumerable<UserRoleResultDTO>> GetUserRolesAsync(Guid userId, Guid? schoolId);
        Task<IEnumerable<UserRole>> GetUserRolesDataAsync(Guid userId, Guid? schoolId);
        Task<IEnumerable<UserRoleResultDTO>> GetRoleUsersAsync(Guid roleId, Guid? schoolId);
        Task<IEnumerable<UserRoleResultDTO>> GetSchoolsAdminsAsync();

        Task<IEnumerable<UserRoleResultDTO>> GetApplicationManagersAsync(string name, string phoneNumber);
    }
}
