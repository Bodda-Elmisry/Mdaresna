using Mdaresna.Doamin.DTOs.ClassRoomManagement;
using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.ClassRoomManagement.Query
{
    public class ClassRoomAssignmentQueryRepository : BaseQueryRepository<ClassRoomAssignment>, IClassRoomAssignmentQueryRepository
    {
        private readonly AppDbContext context;
        private readonly AppSettingDTO _appSettings;

        public ClassRoomAssignmentQueryRepository(AppDbContext context,
                                                IOptions<AppSettingDTO> appSettings) 
            : base(context)
        {
            this.context = context;
            this._appSettings = appSettings.Value;
        }

        public async Task<IEnumerable<ClassRoomAssignmentResultDTO>> GetClassRoomAssignmentsList(Guid? classRoomId, Guid? SupervisorId, Guid? courseId, string? details, decimal? rate, DateTime? fromdate, DateTime? todate, int pageNumber)
        {
            int pagesize = _appSettings.PageSize != null ? _appSettings.PageSize.Value : 30;
            var query = context.ClassRoomAssignments
                        .Include(c => c.ClassRoom).Include(c => c.Course).Include(c => c.Supervisor)
                        .Select(c => new ClassRoomAssignmentResultDTO
                        {
                            Id = c.Id,
                            SupervisorId = c.SupervisorId,
                            SupervisorName = $"{c.Supervisor.FirstName} {c.Supervisor.MiddelName} {c.Supervisor.LastName}",
                            CourseId = c.CourseId,
                            CourseName = c.Course.Name,
                            ClassRoomId = c.ClassRoomId,
                            ClassRoomName = c.ClassRoom.Name,
                            AssignmentDate = c.AssignmentDate,
                            Details = c.Details,
                            WeekDay = c.WeekDay,
                            Rate = c.Rate,
                            CreateDate = c.CreateDate,
                            LastModifyDate = c.LastModifyDate,
                        });

            if (classRoomId != null)
                query = query.Where(q => q.ClassRoomId == classRoomId);

            if (SupervisorId != null)
                query = query.Where(q => q.SupervisorId == SupervisorId);

            if (courseId != null)
                query = query.Where(q => q.CourseId == courseId);

            if (!string.IsNullOrEmpty(details))
                query = query.Where(q => q.Details.Contains(details));

            if (rate != null)
                query = query.Where(q => q.Rate == rate);

            fromdate = fromdate != null ? new DateTime(fromdate.Value.Year, fromdate.Value.Month, fromdate.Value.Day) : null;
            todate = todate != null ? new DateTime(todate.Value.Year, todate.Value.Month, todate.Value.Day) : null;

            if (fromdate != null && todate != null && fromdate != todate)
                query = query.Where(q => q.AssignmentDate >= fromdate && q.AssignmentDate <= todate);

            query = query.OrderByDescending(q=> q.AssignmentDate)
                         .Skip((pageNumber - 1)* pagesize)
                         .Take(pagesize);

            return await query.ToListAsync();




        }

        public async Task<ClassRoomAssignmentResultDTO?> GetClassRoomAssignmentById(Guid assignmentId)
        {
            var item = await context.ClassRoomAssignments.Include(a=> a.Supervisor)
                                                         .Include(a => a.Course)
                                                         .Include(a => a.ClassRoom)
                                                         .FirstOrDefaultAsync(c=> c.Id == assignmentId);

            return item != null ?
                new ClassRoomAssignmentResultDTO
                {
                    Id = item.Id,
                    SupervisorId = item.SupervisorId,
                    SupervisorName = $"{item.Supervisor.FirstName} {item.Supervisor.MiddelName} {item.Supervisor.LastName}",
                    CourseId = item.CourseId,
                    CourseName = item.Course.Name,
                    ClassRoomId = item.ClassRoomId,
                    ClassRoomName = item.ClassRoom.Name,
                    AssignmentDate = item.AssignmentDate,
                    Details = item.Details,
                    WeekDay = item.WeekDay,
                    Rate = item.Rate,
                    CreateDate = item.CreateDate,
                    LastModifyDate = item.LastModifyDate,
                } : null;
        }



    }
}
