using Mdaresna.Doamin.DTOs.ClassRoomManagement;
using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.Models.Base.Relation;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.ClassRoomManagement.Query
{
    public class ClassRoomExamQueryRepository : BaseQueryRepository<ClassRoomExam>, IClassRoomExamQueryRepository
    {
        private readonly AppDbContext context;

        public ClassRoomExamQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<ClassRoomExamResultDTO>> GetExamsList(IEnumerable<Guid> months, DateTime? fromDate, DateTime? toDate,
                                                                      string weekDay, Guid? classRoomId, Guid? supervisorId,
                                                                      Guid? courseId, decimal? rate)
        {
            var query = context.ClassRoomExams.AsQueryable();

            if(months != null && months.Count() > 0)
                query = query.Where(e => months.Contains(e.MonthId));

            if (fromDate != null && toDate != null)
                query = query.Where(e => e.ExamDate >= fromDate && e.ExamDate <= toDate);

            else if (fromDate != null)
                query = query.Where(e => e.ExamDate == fromDate);

            if (!string.IsNullOrEmpty(weekDay))
                query = query.Where(e => e.WeekDay == weekDay);

            if (classRoomId != null)
                query = query.Where(e => e.ClassRoomId == classRoomId);

            if (supervisorId != null)
                query = query.Where(e => e.SupervisorId == supervisorId);

            if (courseId != null)
                query = query.Where(e => e.CourseId == courseId);


            if (rate != null)
                query = query.Where(e => e.Rate == rate);

            var queryString = query.ToQueryString();

            var result = await query.Select(e =>
            new ClassRoomExamResultDTO
            {
                Id = e.Id,
                ExamDate = e.ExamDate,
                WeekDay = e.WeekDay,
                ExamDetails = e.Details,
                ClassRoomId = e.ClassRoomId,
                ClassRoom = e.ClassRoom.Name,
                SupervisorId = e.SupervisorId,
                SupervisorName = string.Format("{0} {1} {2}", e.Supervisor.FirstName, e.Supervisor.MiddelName, e.Supervisor.LastName),
                MonthId = e.MonthId,
                Month = e.Month.Name,
                CourseId = e.CourseId,
                CourseName = e.Course.Name,
                Rate = e.Rate
            }).ToListAsync();

            return result;

        }

        public async Task<CreateClassRoomExamInitialDataDTO> GetInitialData(Guid schoolId)
        {
            var supervisors = await context.schoolTeachers.Where(t => t.SchoolId == schoolId)
                                            .Select(t => new DropDownDTO
                                            {
                                                Id = t.TeacherId,
                                                Name = string.Format("{0} {1} {2}", t.Teacher.FirstName, t.Teacher.MiddelName, t.Teacher.LastName)
                                            }).ToListAsync();

            var classRooms = await context.ClassRooms.Where(t => t.SchoolId == schoolId)
                                            .Select(t => new DropDownDTO
                                            {
                                                Id = t.Id,
                                                Name = t.Name
                                            }).ToListAsync();

            var courses = await context.SchoolCourses.Where(t => t.SchoolId == schoolId)
                                            .Select(t => new DropDownDTO
                                            {
                                                Id = t.Id,
                                                Name = t.Name
                                            }).ToListAsync();

            var coursesMonthes = await context.SchoolYearMonths.Where(t => t.Year.SchoolId == schoolId)
                                            .Select(t => new DropDownDTO
                                            {
                                                Id = t.Id,
                                                Name = t.Name
                                            }).ToListAsync();

            var result = new CreateClassRoomExamInitialDataDTO
            {
                ClassRooms = classRooms,
                Courses = courses,
                CoursesMonthes = coursesMonthes,
                Supervisors = supervisors
            };

            return result;

        }

        public async Task<ClassRoomExamResultDTO?> GetExamByIdAsync(Guid examid)
        {
            var result = await context.ClassRoomExams.Include(e=> e.ClassRoom).Include(e => e.Supervisor).Include(e => e.Course).Include(e => e.Month).FirstOrDefaultAsync(e=> e.Id == examid);

            return result != null ? new ClassRoomExamResultDTO
            {
                Id = result.Id,
                ExamDate = result.ExamDate,
                WeekDay = result.WeekDay,
                ExamDetails = result.Details,
                ClassRoomId = result.ClassRoomId,
                ClassRoom = result.ClassRoom.Name,
                SupervisorId = result.SupervisorId,
                SupervisorName = string.Format("{0} {1} {2}", result.Supervisor.FirstName, result.Supervisor.MiddelName, result.Supervisor.LastName),
                MonthId = result.MonthId,
                Month = result.Month.Name,
                CourseId = result.CourseId,
                CourseName = result.Course.Name,
                Rate = result.Rate
            } : null;
        }







    }
}
