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
            return await context.UserRoles.AnyAsync(u => u.RoleId == userRole.RoleId && u.UserId == userRole.UserId && u.Deleted == false);

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
            var query = context.UserRoles.Include(u => u.User)
                                         .Include(u => u.Role)
                                         .Include(u => u.School)
                                         .Where(u => u.RoleId == Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73") && u.Deleted == false);
            

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

        public async Task<UserRoleResultDTO?> GetUserRoleAsync(Guid userId, Guid roleId)
        {
            var userRole = await context.UserRoles.Include(u => u.User)
                                         .Include(u => u.Role)
                                         .Include(u => u.School)
                                         .FirstOrDefaultAsync(u => u.UserId == userId && u.RoleId == roleId && u.Deleted == false);

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


    }
}
