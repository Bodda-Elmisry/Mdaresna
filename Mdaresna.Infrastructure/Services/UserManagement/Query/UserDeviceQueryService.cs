using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.UserManagement.Query;
using Mdaresna.Repository.IServices.UserManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.UserManagement.Query
{
    public class UserDeviceQueryService : BaseQueryService<UserDevice>, IUserDeviceQueryService
    {
        private readonly IUserDeviceQueryRepository userDeviceQueryRepository;

        public UserDeviceQueryService(IBaseQueryRepository<UserDevice> queryRepository, 
                                      IBaseSharedRepository<UserDevice> sharedRepository,
                                      IUserDeviceQueryRepository userDeviceQueryRepository) : base(queryRepository, sharedRepository)
        {
            this.userDeviceQueryRepository = userDeviceQueryRepository;
        }

        public async Task<bool> CheckIfUserFcmTokenExistAsync(Guid userId, string fcmTocken)
        {
            return await userDeviceQueryRepository.CheckIfUserFcmTokenExistAsync(userId, fcmTocken);
        }

        public async Task<UserDevice?> GetByDeviceIdAsync(string deviceId)
        {
            return await userDeviceQueryRepository.GetByDeviceIdAsync(deviceId);
        }

        public async Task<UserDevice?> GetByUserIdAndFcmTockenAsync(Guid userId, string fcmTocken)
        {
            return await userDeviceQueryRepository.GetByUserIdAndFcmTockenAsync(userId, fcmTocken);
        }

        public async Task<IEnumerable<UserDevice>> GetByUserIdAsync(Guid userId)
        {
            return await userDeviceQueryRepository.GetByUserIdAsync(userId);
        }

        public async Task<IEnumerable<UserDevice>> GetUsersDevicesAsync(IEnumerable<Guid> userIds)
        {
            return await userDeviceQueryRepository.GetUsersDevicesAsync(userIds);
        }
    }
}
