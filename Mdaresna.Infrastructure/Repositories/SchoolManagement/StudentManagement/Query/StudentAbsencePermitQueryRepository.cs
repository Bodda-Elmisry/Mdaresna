using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.StudentManagement.Query
{
    public class StudentAbsencePermitQueryRepository : BaseQueryRepository<StudentAbsencePermit>, IStudentAbsencePermitQueryRepository
    {
        private readonly AppDbContext context;
        private readonly AppSettingDTO appSettings;

        public StudentAbsencePermitQueryRepository(
            AppDbContext context,
            IOptions<AppSettingDTO> appSettings) : base(context)
        {
            this.context = context;
            this.appSettings = appSettings.Value;
        }

        public async Task<StudentAbsencePermit?> GetActivePermitAsync(Guid studentId, DateTime date)
        {
            var permitDate = date.Date;
            return await context.StudentAbsencePermits
                .FirstOrDefaultAsync(p =>
                    p.Deleted == false &&
                    p.StudentId == studentId &&
                    p.Date.Date == permitDate);
        }

        public async Task<IEnumerable<StudentAbsencePermitResultDTO>> GetStudentAbsencePermitsAsync(
            Guid? studentId,
            Guid? parentId,
            int pageNumber)
        {
            var pageSize = appSettings.PageSize ?? 30;
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;

            var query = context.StudentAbsencePermits
                .Include(p => p.Student)
                .Include(p => p.Parent)
                .Include(p => p.ClassRoom)
                .Where(p => p.Deleted == false)
                .Select(p => new StudentAbsencePermitResultDTO
                {
                    Id = p.Id,
                    StudentId = p.StudentId,
                    StudentName = p.Student != null
                        ? $"{p.Student.FirstName} {p.Student.MiddelName} {p.Student.LastName}"
                        : string.Empty,
                    ParentId = p.ParentId,
                    ParentName = p.Parent != null
                        ? $"{p.Parent.FirstName} {p.Parent.MiddelName} {p.Parent.LastName}"
                        : string.Empty,
                    ClassRoomId = p.ClassRoomId,
                    ClassRoomName = p.ClassRoom != null ? p.ClassRoom.Name : string.Empty,
                    Date = p.Date,
                    Reason = p.Reason
                });

            if (studentId != null && studentId != Guid.Empty)
            {
                query = query.Where(p => p.StudentId == studentId);
            }

            if (parentId != null && parentId != Guid.Empty)
            {
                query = query.Where(p => p.ParentId == parentId);
            }

            return await query
                .OrderByDescending(p => p.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
