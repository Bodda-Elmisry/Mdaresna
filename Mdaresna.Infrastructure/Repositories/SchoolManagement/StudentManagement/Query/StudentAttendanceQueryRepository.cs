using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.StudentManagement.Query
{
    public class StudentAttendanceQueryRepository : BaseQueryRepository<StudentAttendance>, IStudentAttendanceQueryRepository
    {
        private readonly AppDbContext context;

        private readonly AppSettingDTO _appSettings;
        public StudentAttendanceQueryRepository(AppDbContext context,
                                                IOptions<AppSettingDTO> appSettings) 
            : base(context)
        {
            this.context = context;
            this._appSettings = appSettings.Value;
        }


        public async Task<IEnumerable<StudentAttendanceResultDTO>> GetStudentsAttendancesAsync(Guid? studentId, Guid? classRoomId, int pageNumber)
        {
            int pagesize = _appSettings.PageSize != null ? _appSettings.PageSize.Value : 30;
            var query = context.StudentAttendances
                        .Include(a=> a.Student).Include(a => a.ClassRoom).Include(a => a.Supervisor)
                        .Select(a => new StudentAttendanceResultDTO
                        {
                            Id = a.Id,
                            Date = a.Date,
                            WeekDay = a.WeekDay,
                            ClassRoomId = a.ClassRoomId,
                            ClassRoomName = (a != null && a.ClassRoom != null) ?  a.ClassRoom.Name : string.Empty,
                            SupervisorId = a.SupervisorId,
                            SupervisorName = (a != null && a.Supervisor != null) ? $"{a.Supervisor.FirstName} {a.Supervisor.MiddelName} {a.Supervisor.LastName}" : string.Empty,
                            StudentId = a.StudentId,
                            StudentName = (a != null && a.Student != null) ? $"{a.Student.FirstName} {a.Student.MiddelName} {a.Student.LastName}" : string.Empty,
                            IsAttend = a.IsAttend
                        });

            if (studentId != null)
            {
                query = query.Where(a=> a.StudentId == studentId);
            }

            if (classRoomId != null)
            {
                query = query.Where(a => a.ClassRoomId == classRoomId);
            }

            query = query.OrderByDescending(a => a.Date)
                         .Skip((pageNumber - 1) * pagesize)
                         .Take(pagesize);

            return await query.ToListAsync();

        }




    }
}
