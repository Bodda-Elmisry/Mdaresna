using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.UserManagement.Query
{
    public interface IUserDeviceQueryRepository : IBaseQueryRepository<UserDevice>
    {
        Task<UserDevice?> GetByDeviceIdAsync(string deviceId);
        Task<UserDevice?> GetByUserIdAndFcmTockenAsync(Guid userId, string fcmTocken);
        Task<IEnumerable<UserDevice>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<UserDevice>> GetUsersDevicesAsync(IEnumerable<Guid> userIds);
        Task<bool> CheckIfUserFcmTokenExistAsync(Guid userId, string fcmTocken);
    }
}
