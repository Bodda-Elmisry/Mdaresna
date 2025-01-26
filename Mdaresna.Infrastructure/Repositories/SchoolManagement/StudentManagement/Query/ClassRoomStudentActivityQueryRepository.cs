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
    public class ClassRoomStudentActivityQueryRepository : BaseQueryRepository<ClassRoomStudentActivity>, IClassRoomStudentActivityQueryRepository
    {
        private readonly AppDbContext context;
        private readonly AppSettingDTO appSettings;

        public ClassRoomStudentActivityQueryRepository(AppDbContext context,
                                                IOptions<AppSettingDTO> appSettings) : base(context)
        {
            this.context = context;
            this.appSettings = appSettings.Value;
        }

        public async Task<IEnumerable<ClassRoomStudentActivityResultDTO>> GetStudentActivitiesListAsync(Guid? StudentId,
                                                                                                     Guid? ActivityId,
                                                                                                     decimal? ResultFrom,
                                                                                                     decimal? ResultTo, 
                                                                                                     int pageNumber)
        {
            int pagesize = appSettings.PageSize != null ? appSettings.PageSize.Value : 30;
            #region Create Query
            var query = context.ClassRoomStudentActivities.Include(s => s.Student).Include(s => s.Activity)
                                                           .Include(s => s.Activity.Course)
                                                           .Include(s => s.Activity.ClassRoom)
                                                           .Include(s => s.Activity.Supervisor)
                                                           .AsQueryable();

            if (StudentId != null)
                query = query.Where(s => s.StudentId == StudentId);

            if (ActivityId != null)
                query = query.Where(s => s.ActivityId == ActivityId);

            if (ResultFrom != null && ResultTo != null)
                query = query.Where(s => s.Result >= ResultFrom && s.Result <= ResultTo);
            else if (ResultFrom != null && ResultTo == null)
                query = query.Where(s => s.Result == ResultFrom);




            #endregion

            query = query.OrderByDescending(q => q.Activity.ActivityDate)
                         .Skip((pageNumber - 1) * pagesize)
                         .Take(pagesize);

            var result = await query
                                .Select(s => new ClassRoomStudentActivityResultDTO
                                {
                                    ActivityId = s.ActivityId,
                                    ActivityDate = s.Activity.ActivityDate,
                                    WeekDay = s.Activity.WeekDay,
                                    ActivityDetails = s.Activity.Details,
                                    CourseId = s.Activity.CourseId,
                                    CourseName = s.Activity.Course.Name,
                                    ClassRoomId = s.Activity.ClassRoomId,
                                    ClassRoom = s.Activity.ClassRoom.Name,
                                    SupervisorId = s.Activity.SupervisorId,
                                    SupervisorName = $"{s.Activity.Supervisor.FirstName} {s.Activity.Supervisor.MiddelName} {s.Activity.Supervisor.LastName}",
                                    ActivityRate = s.Activity.Rate,
                                    StudentId = s.StudentId,
                                    StudentName = $"{s.Student.FirstName} {s.Student.MiddelName} {s.Student.LastName}",
                                    Result = s.Result
                                })
                                .ToListAsync();

            return result;
        }

        public async Task<ClassRoomStudentActivity?> GetClassRoomStudentActivityAsync(Guid studentId, Guid ActivityId)
        {
            return await context.ClassRoomStudentActivities.FirstOrDefaultAsync(s => s.StudentId == studentId && s.ActivityId == ActivityId);
        }

        public async Task<ClassRoomStudentActivityResultDTO?> GetClassRoomStudentActivityViewAsync(Guid studentId, Guid ActivityId)
        {
            var Activity = await context.ClassRoomStudentActivities.Include(s => s.Student).Include(s => s.Activity)
                                                           .Include(s => s.Activity.Course)
                                                           .Include(s => s.Activity.ClassRoom)
                                                           .Include(s => s.Activity.Supervisor)
                                                           .FirstOrDefaultAsync(s => s.StudentId == studentId && s.ActivityId == ActivityId);

            return Activity == null ? null : new ClassRoomStudentActivityResultDTO
            {
                ActivityId = Activity.ActivityId,
                ActivityDate = Activity.Activity.ActivityDate,
                WeekDay = Activity.Activity.WeekDay,
                ActivityDetails = Activity.Activity.Details,
                CourseId = Activity.Activity.CourseId,
                CourseName = Activity.Activity.Course.Name,
                ClassRoomId = Activity.Activity.ClassRoomId,
                ClassRoom = Activity.Activity.ClassRoom.Name,
                SupervisorId = Activity.Activity.SupervisorId,
                SupervisorName = $"{Activity.Activity.Supervisor.FirstName} {Activity.Activity.Supervisor.MiddelName} {Activity.Activity.Supervisor.LastName}",
                ActivityRate = Activity.Activity.Rate,
                StudentId = Activity.StudentId,
                StudentName = $"{Activity.Student.FirstName} {Activity.Student.MiddelName} {Activity.Student.LastName}",
                Result = Activity.Result,
                IsAttend = Activity.IsAttend
            };
        }










    }
}
