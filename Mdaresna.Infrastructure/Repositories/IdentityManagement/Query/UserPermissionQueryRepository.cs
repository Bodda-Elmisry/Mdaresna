using Mdaresna.Doamin.DTOs.Identity;
using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
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
    public class UserPermissionQueryRepository : BaseQueryRepository<UserPermission>, IUserPermissionQueryRepository
    {
        private readonly AppDbContext context;

        public UserPermissionQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<UserPermission?> GetUserPermissionByID(Guid permissionId, Guid schoolId,Guid UserId)
        {
            return await context.userPermissions.FirstOrDefaultAsync(p=> p.UserId == UserId && p.SchoolId == schoolId && p.UserId == UserId && p.Deleted == false);
        }

        public async Task<IEnumerable<UserPermission>?> GetUserPermissions(Guid schoolId, Guid UserId)
        {
            return await context.userPermissions.Where(p => p.UserId == UserId && p.SchoolId == schoolId && p.UserId == UserId && p.Deleted == false).ToListAsync();
        }

        public async Task<IEnumerable<Permission>> GetUserPermissions(Guid UserId)
        {
            var query = from rp in context.RolePermissions
                        join p in context.Permissions on rp.PermissionId equals p.Id
                        join ur in (
                            from userRole in context.UserRoles
                            join schoolUser in context.SchoolUsers.OrderByDescending(su => su.SchoolId).Take(1)
                            on userRole.SchoolId equals schoolUser.SchoolId into schoolUsers
                            from su in schoolUsers.DefaultIfEmpty()
                            where userRole.UserId == UserId && userRole.Deleted == false
                            select userRole.RoleId
                    ) on rp.RoleId equals ur
                        select p;

            /*
             var query = from p in context.RolePermissions
            join perm in context.Permissions on p.PermissionId equals perm.Id
            join ur in (from userRole in context.UserRoles
                        join schoolUser in (from su in context.SchoolUsers
                                            orderby su.SchoolId
                                            select su.SchoolId).Take(1) on userRole.SchoolId equals schoolUser
                        where userRole.UserId == "01AB4B99-CF6A-4BC2-92DA-85037D7D7F9E").DefaultIfEmpty() on ur.RoleId equals p.RoleId
            select p;             
              */

            var result = await query.ToListAsync();
            return result;
        }



        public async Task<IEnumerable<UserPermissionResultDTO>> GetUserPermissionsView(Guid userId, Guid? schoolID)
        {
            var query1 = from ur in context.UserRoles
                         join u in context.Users on ur.UserId equals u.Id
                         join r in context.Roles on ur.RoleId equals r.Id
                         join s in context.Schools on ur.SchoolId equals s.Id into schoolsGroup
                         from s in schoolsGroup.DefaultIfEmpty() // left outer join
                         join rp in context.RolePermissions on ur.RoleId equals rp.RoleId
                         join p in context.Permissions on rp.PermissionId equals p.Id
                         where u.Id == userId && ur.Deleted == false
                         select new
                         {
                             UserId = u.Id,
                             u.UserName,
                             FullName = u.FirstName + " " + u.MiddelName + " " + u.LastName,
                             RoleId = (Guid?)r.Id,
                             RoleName = r.Name,
                             SchoolId = s.Id,
                             SchoolName = s.Name,
                             permssionId = p.Id,
                             p.Key,
                             p.Name,
                             p.Name_AR,
                             p.SchoolPermission,
                             p.AppPermission,
                             p.AllowedToMapToClassroom
                         };

            var query2 = from up in context.userPermissions
                         join u in context.Users on up.UserId equals u.Id
                         join s in context.Schools on up.SchoolId equals s.Id
                         join p in context.Permissions on up.PermissionId equals p.Id
                         where u.Id == userId && up.Deleted == false
                         select new
                         {
                             UserId = u.Id,
                             u.UserName,
                             FullName = u.FirstName + " " + u.MiddelName + " " + u.LastName,
                             RoleId = (Guid?)null,
                             RoleName = "Extra",
                             SchoolId = s.Id,
                             SchoolName = s.Name,
                             permssionId = p.Id,
                             p.Key,
                             p.Name,
                             p.Name_AR,
                             p.SchoolPermission,
                             p.AppPermission,
                             p.AllowedToMapToClassroom
                         };


            var combinedQuery = query1.Union(query2).Distinct();
            var query = combinedQuery.ToQueryString();

            if (schoolID != null)
                combinedQuery = combinedQuery.Where(p=> p.SchoolId == schoolID || p.SchoolId == null);

            var querystring = combinedQuery.ToQueryString();

            

            var result = await combinedQuery.Select(s => new UserPermissionResultDTO
            {
                UserId = s.UserId,
                UserName = s.UserName,
                UserFullName = s.FullName,
                RoleId = s.RoleId,
                RoleName = s.RoleName,
                SchoolId = s.SchoolId,
                SchoolName = s.SchoolName,
                PermissionId = s.permssionId,
                PermissionKey = s.Key,
                PermissionName = s.Name,
                PermissionName_AR = s.Name_AR,
                AppPermission = s.AppPermission,
                SchoolPermission = s.SchoolPermission,
                AllowMapToClassroom = s.AllowedToMapToClassroom
            }).ToListAsync();


            var userIds = result.Select(r=> r.UserId).ToList();

            var permissionIds = result.Select(r=> r.PermissionId).ToList();

            //var userPermissionsClassrooms = context.userPermissionSchoolClassRooms
            //                                .Include(u=> u.ClassRoom)
            //                                .se
            //                                .Where(u=> userIds.Contains(u.UserId) && permissionIds.Contains(u.PermissionId)).ToList();

            var userPermissionsClassroomsQuery = from upc in context.userPermissionSchoolClassRooms
                                            join c in context.ClassRooms on upc.ClassRoomId equals c.Id
                                            where permissionIds.Contains(upc.PermissionId)
                                                  && userIds.Contains(upc.UserId)
                                            select new
                                            {
                                                ClassroomName = c.Name,
                                                ClassroomId = upc.ClassRoomId,
                                                UserId = upc.UserId,
                                                PermissionId = upc.PermissionId
                                            };
            var userPermissionsClassrooms = userPermissionsClassroomsQuery.ToList();

            foreach(var r in result)
            {
                r.Classrooms = userPermissionsClassrooms.Where(c=> c.PermissionId == r.PermissionId &&  c.UserId == r.UserId).Select(c=> c.ClassroomId).ToList();
            }

            return result;


        }




    }
}
