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
    public interface IUserPermissionQueryService : IBaseQueryService<UserPermission>
    {
        Task<IEnumerable<Permission>> GetUserPermissions(Guid UserId);
        Task<IEnumerable<UserPermission>?> GetUserPermissions(Guid schoolId, Guid UserId);
        Task<IEnumerable<UserPermissionResultDTO>> GetUserPermissionsView(Guid userId, Guid? schoolID);
        Task<UserPermission?> GetUserPermissionByID(Guid permissionId, Guid schoolId, Guid UserId);
    }
}
