using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.IdentityManagement.Query
{
    public interface IUserPermissionSchoolClassRoomQueryService : IBaseQueryService<UserPermissionSchoolClassRoom>
    {
        Task<UserPermissionSchoolClassRoom?> GetUserPermissionSchoolClassRoomByIdAsync(Guid userId, Guid permissionId, Guid classroomId);
    }
}
