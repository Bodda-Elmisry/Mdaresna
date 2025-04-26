using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.IdentityManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.IdentityManagement.Query
{
    public class UserPermissionSchoolClassRoomQueryRepository : BaseQueryRepository<UserPermissionSchoolClassRoom>, IUserPermissionSchoolClassRoomQueryRepository
    {
        private readonly AppDbContext context;

        public UserPermissionSchoolClassRoomQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<UserPermissionSchoolClassRoom?> GetUserPermissionSchoolClassRoomByIdAsync(Guid userId, Guid permissionId, Guid classroomId)
        {
            return await context.userPermissionSchoolClassRooms.FirstOrDefaultAsync(c => c.UserId == userId && c.PermissionId == permissionId && c.ClassRoomId == classroomId && c.Deleted == false);
        }


    }
}
