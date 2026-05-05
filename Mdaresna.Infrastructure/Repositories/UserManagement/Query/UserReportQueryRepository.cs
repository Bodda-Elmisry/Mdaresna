using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.DTOs.UserManagement;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.UserManagement.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Mdaresna.Infrastructure.Repositories.UserManagement.Query
{
    public class UserReportQueryRepository : BaseQueryRepository<UserReport>, IUserReportQueryRepository
    {
        private readonly AppDbContext context;
        private readonly AppSettingDTO appSettings;

        public UserReportQueryRepository(AppDbContext context,
                                         IOptions<AppSettingDTO> appSettings) : base(context)
        {
            this.context = context;
            this.appSettings = appSettings.Value;
        }

        public async Task<IEnumerable<UserReportResultDTO>> GetUserReportsAsync(Guid reportedUserId)
        {
            return await context.UserReports
                .Include(r => r.ReporterUser)
                .Include(r => r.ReportedUser)
                .Where(r => r.ReportedUserId == reportedUserId && r.Deleted == false)
                .Select(r => new UserReportResultDTO
                {
                    Id = r.Id,
                    ReporterUserId = r.ReporterUserId,
                    ReporterUserName = r.ReporterUser.UserName,
                    ReporterFullName = $"{r.ReporterUser.FirstName} {r.ReporterUser.LastName}",
                    ReporterPhoneNumber = r.ReporterUser.PhoneNumber,
                    ReportedUserId = r.ReportedUserId,
                    ReportedUserName = r.ReportedUser.UserName,
                    ReportedFullName = $"{r.ReportedUser.FirstName} {r.ReportedUser.LastName}",
                    ReportedUserPhoneNumber = r.ReportedUser.PhoneNumber,
                    Report = r.Description
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<UserReportsCountResultDTO>> GetUsersWithReportsCountAsync(Guid? schoolId, string? userName, int? minReportsCount, int? maxReportsCount, int pageNumber)
        {
            int pageSize = appSettings.PageSize ?? 30;
            var usersQuery = context.Users.Where(u => u.Deleted == false);

            if (schoolId.HasValue)
            {
                var selectedSchoolId = schoolId.Value;
                var schoolUserIds = context.SchoolUsers
                    .Where(su => su.Deleted == false && su.SchoolId == selectedSchoolId)
                    .Select(su => su.UserId)
                    .Union(
                        context.SchoolEmployees
                            .Where(se => se.Deleted == false && se.SchoolId == selectedSchoolId)
                            .Select(se => se.EmployeeId))
                    .Union(
                        context.schoolTeachers
                            .Where(st => st.Deleted == false && st.SchoolId == selectedSchoolId)
                            .Select(st => st.TeacherId))
                    .Union(
                        context.UserRoles
                            .Where(ur => ur.Deleted == false && ur.SchoolId == selectedSchoolId)
                            .Select(ur => ur.UserId))
                    .Union(
                        context.StudentParents
                            .Where(sp => sp.Deleted == false &&
                                         sp.Student.Deleted == false &&
                                         sp.Student.SchoolId == selectedSchoolId)
                            .Select(sp => sp.ParentId));

                usersQuery = usersQuery.Where(u => schoolUserIds.Contains(u.Id));
            }

            if (!string.IsNullOrWhiteSpace(userName))
            {
                usersQuery = usersQuery.Where(u =>
                    (u.UserName != null && u.UserName.Contains(userName)) ||
                    u.FirstName.Contains(userName) ||
                    u.LastName.Contains(userName));
            }

            var usersWithReports = usersQuery
                .GroupJoin(context.UserReports.Where(r => r.Deleted == false),
                           user => user.Id,
                           report => report.ReportedUserId,
                           (user, reports) => new
                           {
                               User = user,
                               ReportsCount = reports.Count()
                           })
                .Where(x => x.ReportsCount > 0);

            if (minReportsCount.HasValue)
            {
                usersWithReports = usersWithReports.Where(x => x.ReportsCount >= minReportsCount.Value);
            }

            if (maxReportsCount.HasValue)
            {
                usersWithReports = usersWithReports.Where(x => x.ReportsCount <= maxReportsCount.Value);
            }

            return await usersWithReports
                .OrderByDescending(x => x.ReportsCount)
                .ThenBy(x => x.User.FirstName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new UserReportsCountResultDTO
                {
                    UserId = x.User.Id,
                    UserName = x.User.UserName,
                    FullName = $"{x.User.FirstName} {x.User.LastName}",
                    PhoneNumber = x.User.PhoneNumber,
                    ReportsCount = x.ReportsCount,
                    LastModifyDate = x.User.LastModifyDate
                })
                .ToListAsync();
        }
    }
}
