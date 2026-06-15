using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
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

        public async Task<IEnumerable<UserPermissionSchoolClassRoom>> GetUserPermissionsBySchoolAsync(Guid userId, Guid schoolId)
        {
            return await context.userPermissionSchoolClassRooms
                .Join(context.Set<ClassRoom>(),
                    up => up.ClassRoomId,
                    c => c.Id,
                    (up, c) => new { up, c })
                .Where(x => x.up.UserId == userId && x.c.SchoolId == schoolId && x.up.Deleted == false)
                .Select(x => x.up)
                .ToListAsync();
        }
    }
}
