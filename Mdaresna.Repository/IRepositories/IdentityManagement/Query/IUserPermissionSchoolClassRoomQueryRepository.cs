using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.IdentityManagement.Query
{
    public interface IUserPermissionSchoolClassRoomQueryRepository : IBaseQueryRepository<UserPermissionSchoolClassRoom>
    {
        Task<UserPermissionSchoolClassRoom?> GetUserPermissionSchoolClassRoomByIdAsync(Guid userId, Guid permissionId, Guid classroomId);
        Task<IEnumerable<UserPermissionSchoolClassRoom>> GetUserPermissionsBySchoolAsync(Guid userId, Guid schoolId);
        Task<bool> HasClassRoomPermissionsAsync(Guid classroomId);
    }
}
