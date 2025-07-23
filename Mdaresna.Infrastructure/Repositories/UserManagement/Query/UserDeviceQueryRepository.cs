using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.UserManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.UserManagement.Query
{
    public class UserDeviceQueryRepository : BaseQueryRepository<UserDevice>, IUserDeviceQueryRepository
    {
        private readonly AppDbContext context;

        public UserDeviceQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<UserDevice?> GetByDeviceIdAsync(string deviceId)
        {
            return await context.UserDevices
                .Where(ud => ud.DeviceId == deviceId)
                .FirstOrDefaultAsync();
        }

        public async Task<UserDevice?> GetByUserIdAndFcmTockenAsync(Guid userId, string fcmTocken)
        {
            return await context.UserDevices
                .Where(ud => ud.UserId == userId && ud.FcmToken == fcmTocken)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<UserDevice>> GetByUserIdAsync(Guid userId)
        {
            return await context.UserDevices
                .Where(ud => ud.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<UserDevice>> GetUsersDevicesAsync(IEnumerable<Guid> userIds)
        {
            return await context.UserDevices
                .Where(ud => userIds.Contains(ud.UserId)).ToListAsync();
        }

        public async Task<bool> CheckIfUserFcmTokenExistAsync(Guid userId, string fcmTocken)
        {
            return await context.UserDevices.AnyAsync(d=> d.UserId == userId && d.FcmToken == fcmTocken);
        }


    }
}
