using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.UserManagement.Query
{
    public interface IUserDeviceQueryService : IBaseQueryService<UserDevice>
    {
        Task<UserDevice?> GetByDeviceIdAsync(string deviceId);
        Task<UserDevice?> GetByUserIdAndFcmTockenAsync(Guid userId, string fcmTocken);
        Task<IEnumerable<UserDevice>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<UserDevice>> GetUsersDevicesAsync(IEnumerable<Guid> userIds);
        Task<bool> CheckIfUserFcmTokenExistAsync(Guid userId, string fcmTocken);
    }
}
