using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Mdaresna.Doamin.DTOs.SchoolManagement;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Query
{
    public class SchoolPostReportQueryRepository : BaseQueryRepository<SchoolPostReport>, ISchoolPostReportQueryRepository
    {
        public SchoolPostReportQueryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<SchoolPostReportResultDTO>> GetSchoolPostReportsAsync(Guid schoolId)
        {
            return await context.SchoolPostReports
                .Include(r => r.User)
                .Include(r => r.Post)
                    .ThenInclude(p => p.School)
                .Where(r => r.Post.SchoolId == schoolId && r.Deleted == false)
                .Select(r => new SchoolPostReportResultDTO
                {
                    Id = r.Id,
                    SchoolId = r.Post.SchoolId,
                    SchoolName = r.Post.School.Name,
                    UserId = r.UserId,
                    UserName = r.User.UserName,
                    UserFullName = $"{r.User.FirstName} {r.User.LastName}",
                    UserPhoneNumber = r.User.PhoneNumber,
                    PostId = r.PostId,
                    Report = r.Description
                }).ToListAsync();
        }
    }
}
