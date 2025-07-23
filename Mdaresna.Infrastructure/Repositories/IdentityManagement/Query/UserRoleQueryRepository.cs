using Mdaresna.Doamin.DTOs.Identity;
using Mdaresna.Doamin.Helpers;
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
    public class UserRoleQueryRepository : BaseQueryRepository<UserRole>, IUserRoleQueryRepository
    {
        private readonly AppDbContext context;

        public UserRoleQueryRepository(AppDbContext context) 
            : base(context)
        {
            this.context = context;
        }

        public async Task<bool> CheckUserRole(UserRole userRole)
        {
            return await context.UserRoles.AnyAsync(u => u.RoleId == userRole.RoleId && u.UserId == userRole.UserId && u.Deleted == false && u.SchoolId == userRole.SchoolId);

        }

        public async Task<IEnumerable<UserRoleResultDTO>> GetUserRolesAsync(Guid userId, Guid? schoolId)
        {
            var query = context.UserRoles.Include(u=> u.User)
                                         .Include(u => u.Role)
                                         .Include(u => u.School)
                                         .Where(u => u.UserId == userId && u.Deleted == false);
            query = schoolId != null ? query.Where(u=> u.SchoolId == schoolId) : query;

            return await query.Select(u => new UserRoleResultDTO
            {
                UserId = u.UserId,
                UserName = u.User.UserName,
                UserFullName = $"{u.User.FirstName} {u.User.MiddelName} {u.User.LastName}",
                UserPhoneNumber = u.User.PhoneNumber,
                RoleId = u.RoleId,
                RoleName = u.Role.Name,
                RoleDescription = u.Role.Description,
                SchoolId = u.SchoolId,
                SchoolName = u.School != null ? u.School.Name : null
            }).ToListAsync();
        }

        public async Task<IEnumerable<UserRole>> GetUserRolesDataAsync(Guid userId, Guid? schoolId)
        {
            var query = context.UserRoles.Where(u => u.UserId == userId && u.Deleted == false);
            query = schoolId != null ? query.Where(u => u.SchoolId == schoolId) : query;

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<UserRoleResultDTO>> GetRoleUsersAsync(Guid roleId, Guid? schoolId)
        {
            var query = context.UserRoles.Include(u => u.User)
                                         .Include(u => u.Role)
                                         .Include(u => u.School)
                                         .Where(u => u.RoleId == roleId && u.Deleted == false);
            query = schoolId != null ? query.Where(u => u.SchoolId == schoolId) : query;

            return await query.Select(u => new UserRoleResultDTO
            {
                UserId = u.UserId,
                UserName = u.User.UserName,
                UserFullName = $"{u.User.FirstName} {u.User.MiddelName} {u.User.LastName}",
                UserImageUrl = !string.IsNullOrEmpty(u.User.ImageUrl) ? $"{SettingsHelper.GetAppUrl()}/{u.User.ImageUrl.Replace("\\", "/")}" : null,
                UserPhoneNumber = u.User.PhoneNumber,
                RoleId = u.RoleId,
                RoleName = u.Role.Name,
                RoleDescription = u.Role.Description,
                SchoolId = u.SchoolId,
                SchoolName = u.School != null ? u.School.Name : null
            }).ToListAsync();
        }

        public async Task<IEnumerable<UserRoleResultDTO>> GetSchoolsAdminsAsync()
        {
            //var query = context.UserRoles.Include(u => u.User)
            //                             .Include(u => u.Role)
            //                             .Include(u => u.School)
            //                             .Where(u => u.RoleId == Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73") && u.Deleted == false);
            //Console.WriteLine(query.ToQueryString());

            //return await query.Select(u => new UserRoleResultDTO
            //{
            //    UserId = u.UserId,
            //    UserName = u.User.UserName,
            //    UserFullName = $"{u.User.FirstName} {u.User.MiddelName} {u.User.LastName}",
            //    UserImageUrl = !string.IsNullOrEmpty(u.User.ImageUrl) ? $"{SettingsHelper.GetAppUrl()}/{u.User.ImageUrl.Replace("\\", "/")}" : null,
            //    UserPhoneNumber = u.User.PhoneNumber,
            //    RoleId = u.RoleId,
            //    RoleName = u.Role.Name,
            //    RoleDescription = u.Role.Description,
            //    SchoolId = u.SchoolId,
            //    SchoolName = u.School != null ? u.School.Name : null
            //}).ToListAsync();

            //var roleId = Guid.Parse("4b8a99fe-b759-4c18-9500-8052c3d7ac73");
            var baseURL = SettingsHelper.GetAppUrl();

            var activeSchoolsCountQuery = from s in context.Schools
                                          where s.Active == true && s.Deleted == false
                                          group s by s.SchoolAdminId into g
                                          select new
                                          {
                                              SchoolAdminId = g.Key,
                                              ActiveSchoolsCount = g.Count()
                                          };

            var query =
                from ur in context.UserRoles
                join u in context.Users on ur.UserId equals u.Id
                join r in context.Roles on ur.RoleId equals r.Id
                join s in context.Schools on ur.SchoolId equals s.Id into schoolGroup
                from school in schoolGroup.DefaultIfEmpty()
                join adsc in activeSchoolsCountQuery on u.Id equals adsc.SchoolAdminId into adscGroup
                from adsc in adscGroup.DefaultIfEmpty()
                where ur.RoleId == Guid.Parse("4b8a99fe-b759-4c18-9500-8052c3d7ac73") && ur.Deleted == false
                select new UserRoleResultDTO
                {
                    UserId = ur.UserId,
                    UserName = u.UserName,
                    UserFullName = $"{u.FirstName} {u.LastName}",
                    UserImageUrl = !string.IsNullOrEmpty(u.ImageUrl) ? $"{SettingsHelper.GetAppUrl()}/{u.ImageUrl}" : null,
                    UserPhoneNumber = u.PhoneNumber,
                    RoleId = ur.RoleId,
                    RoleName = r.Name,
                    RoleDescription = r.Description,
                    SchoolId = ur.SchoolId,
                    SchoolName = school != null ? school.Name : null,
                    ActiveSchoolsCount = adsc.ActiveSchoolsCount == null ? 0 : adsc.ActiveSchoolsCount
                };
                //{
                //    UserId = ur.UserId,
                //    ActiveSchoolsCount = adsc.ActiveSchoolsCount,
                //    u.UserName,
                //    u.FirstName,
                //    u.MiddelName,
                //    u.LastName,
                //    HasImage = !string.IsNullOrEmpty(u.ImageUrl),
                //    ImageUrl = u.ImageUrl != null ? u.ImageUrl.Replace("\\", "/") : null,
                //    u.PhoneNumber,
                //    RoleId = ur.RoleId,
                //    RoleName = r.Name,
                //    RoleDescription = r.Description,
                //    ur.SchoolId,
                //    SchoolName = school != null ? school.Name : null
                //};


            Console.WriteLine(query.ToQueryString());
            return await query.ToListAsync();

            //return await query.Select(u => new UserRoleResultDTO
            //{
            //    UserId = u.UserId,
            //    UserName = u.UserName,
            //    UserFullName = $"{u.FirstName} {u.LastName}",
            //    UserImageUrl = !string.IsNullOrEmpty(u.ImageUrl) ? $"{SettingsHelper.GetAppUrl()}/{u.ImageUrl}" : null,
            //    UserPhoneNumber = u.PhoneNumber,
            //    RoleId = u.RoleId,
            //    RoleName = u.RoleName,
            //    RoleDescription = u.RoleDescription,
            //    SchoolId = u.SchoolId,
            //    SchoolName = u.SchoolName,
            //    ActiveSchoolsCount = u.ActiveSchoolsCount == null ? 0 : u.ActiveSchoolsCount
            //}).ToListAsync();

        }

        public async Task<IEnumerable<UserRoleResultDTO>> GetApplicationManagersAsync(string name, string phoneNumber)
        {
            var query = context.UserRoles.Include(u => u.User)
                                         .Include(u => u.Role)
                                         .Include(u => u.School)
                                         .Where(u => u.Role.AdminRole == true && u.Role.SchoolRole == false && u.Deleted == false && u.RoleId != Guid.Parse("92D00B28-9D25-4BD2-A587-6C22A3A07A92"));

            query = !string.IsNullOrEmpty(name) ? query.Where(u => (u.User.FirstName + u.User.MiddelName + u.User.LastName).Contains(name.Replace(" ", string.Empty))) : query;

            query = !string.IsNullOrEmpty(phoneNumber) ? query.Where(u => u.User.PhoneNumber.Contains(phoneNumber)) : query;


            var querystring = query.ToQueryString();
            Console.WriteLine(querystring);

            return await query.Select(u => new UserRoleResultDTO
            {
                UserId = u.UserId,
                UserName = u.User.UserName,
                UserFullName = $"{u.User.FirstName} {u.User.MiddelName} {u.User.LastName}",
                UserImageUrl = !string.IsNullOrEmpty(u.User.ImageUrl) ? $"{SettingsHelper.GetAppUrl()}/{u.User.ImageUrl.Replace("\\", "/")}" : null,
                UserPhoneNumber = u.User.PhoneNumber,
                RoleId = u.RoleId,
                RoleName = u.Role.Name,
                RoleDescription = u.Role.Description,
                SchoolId = u.SchoolId,
                SchoolName = u.School != null ? u.School.Name : null
            }).ToListAsync();
        }

        public async Task<UserRoleResultDTO?> GetUserRoleViewAsync(Guid userId, Guid roleId, Guid? schoolId)
        {
            var userRoleQuery = context.UserRoles.Include(u => u.User)
                                         .Include(u => u.Role)
                                         .Include(u => u.School)
                                         .Where(u => u.UserId == userId && u.RoleId == roleId && u.Deleted == false);

            userRoleQuery = schoolId != null ? userRoleQuery.Where(u => u.SchoolId == schoolId) : userRoleQuery;

            var userRole = await userRoleQuery.FirstOrDefaultAsync();

            return userRole == null ? null : new UserRoleResultDTO
            {
                UserId = userRole.UserId,
                UserName = userRole.User.UserName,
                UserFullName = $"{userRole.User.FirstName} {userRole.User.MiddelName} {userRole.User.LastName}",
                UserPhoneNumber = userRole.User.PhoneNumber,
                UserImageUrl = !string.IsNullOrEmpty(userRole.User.ImageUrl) ? $"{SettingsHelper.GetAppUrl()}/{userRole.User.ImageUrl.Replace("\\", "/")}" : null,
                RoleId = userRole.RoleId,
                RoleName = userRole.Role.Name,
                RoleDescription = userRole.Role.Description,
                SchoolId = userRole.SchoolId,
                SchoolName = userRole.School != null ? userRole.School.Name : null
            };
        }

        public async Task<UserRole> GetUserRoleAsync(Guid userId, Guid roleId, Guid? schoolId)
        {
            var userRoleQuery = context.UserRoles.Include(u => u.User)
                                         .Include(u => u.Role)
                                         .Include(u => u.School)
                                         .Where(u => u.UserId == userId && u.RoleId == roleId && u.Deleted == false);

            userRoleQuery = schoolId != null ? userRoleQuery.Where(u => u.SchoolId == schoolId) : userRoleQuery;

            var userRole = await userRoleQuery.FirstOrDefaultAsync();

            return userRole;
        }


    }
}
