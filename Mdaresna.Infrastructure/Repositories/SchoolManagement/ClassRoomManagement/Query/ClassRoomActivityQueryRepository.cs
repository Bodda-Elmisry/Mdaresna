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
    public class ClassRoomActivityQueryRepository : BaseQueryRepository<ClassRoomActivity>, IClassRoomActivityQueryRepository
    {
        private readonly AppDbContext context;
        private readonly AppSettingDTO appSettings;

        public ClassRoomActivityQueryRepository(AppDbContext context,
                                                IOptions<AppSettingDTO> appSettings) : base(context)
        {
            this.context = context;
            this.appSettings = appSettings.Value;
        }

        public async Task<IEnumerable<ClassRoomActivityResultDTO>> GetClassRoomActivitiesList(Guid? classRoomId, Guid? SupervisorId, Guid? courseId, string? details, decimal? rate, DateTime? fromdate, DateTime? todate, int pageNumber)
        {
            int pagesize = appSettings.PageSize != null ? appSettings.PageSize.Value : 30;
            var query = context.ClassRoomActivities
                        .Include(c => c.ClassRoom).Include(c => c.Course).Include(c => c.Supervisor).AsQueryable();

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
                query = query.Where(q => q.ActivityDate >= fromdate && q.ActivityDate <= todate);

            query = query.OrderByDescending(q => q.ActivityDate)
                         .Skip((pageNumber - 1) * pagesize)
                         .Take(pagesize);

            var querystring = query.ToQueryString();

            return await query
                        .Select(c => new ClassRoomActivityResultDTO
                        {
                            Id = c.Id,
                            SupervisorId = c.SupervisorId,
                            SupervisorName = $"{c.Supervisor.FirstName} {c.Supervisor.MiddelName} {c.Supervisor.LastName}",
                            CourseId = c.CourseId,
                            CourseName = c.Course.Name,
                            ClassRoomId = c.ClassRoomId,
                            ClassRoom = c.ClassRoom.Name,
                            ActivityDetails = c.Details,
                            WeekDay = c.WeekDay,
                            Rate = c.Rate,
                            CreateDate = c.CreateDate,
                            LastModifyDate = c.LastModifyDate,
                            ActivityDate = c.ActivityDate
                        }).ToListAsync();




        }

        public async Task<ClassRoomActivityResultDTO?> GetClassRoomActivityById(Guid activityId)
        {
            var item = await context.ClassRoomActivities.Include(a=> a.Supervisor)
                                                        .Include(a => a.Course)
                                                        .Include(a => a.ClassRoom)
                                                        .FirstOrDefaultAsync(c => c.Id == activityId);

            return item != null ?
                new ClassRoomActivityResultDTO
                {
                    Id = item.Id,
                    SupervisorId = item.SupervisorId,
                    SupervisorName = $"{item.Supervisor.FirstName} {item.Supervisor.MiddelName} {item.Supervisor.LastName}",
                    CourseId = item.CourseId,
                    CourseName = item.Course.Name,
                    ClassRoomId = item.ClassRoomId,
                    ClassRoom = item.ClassRoom.Name,
                    ActivityDate= item.ActivityDate,
                    ActivityDetails = item.Details,
                    WeekDay = item.WeekDay,
                    Rate = item.Rate,
                    CreateDate = item.CreateDate,
                    LastModifyDate = item.LastModifyDate,
                } : null;
        }














    }
}
