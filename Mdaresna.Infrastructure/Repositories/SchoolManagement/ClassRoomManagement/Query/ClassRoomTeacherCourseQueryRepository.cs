using Mdaresna.Doamin.DTOs.ClassRoomManagement;
using Mdaresna.Doamin.DTOs.Common;
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
    public class ClassRoomTeacherCourseQueryRepository : BaseQueryRepository<ClassRoomTeacherCourse>, IClassRoomTeacherCourseQueryRepository
    {
        private readonly AppDbContext context;

        public ClassRoomTeacherCourseQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<ClassRoomTeacherCourseInitialDTO> GetInitialDataAsync(Guid schoolId)
        {
            


            var coursesQuery = from sc in context.SchoolCourses
                                join ctc in context.ClassRoomTeacherCourses
                                on sc.Id equals ctc.CourseId into sc_ctc
                                from ctc in sc_ctc.DefaultIfEmpty()
                                where sc.SchoolId == schoolId && ctc == null
                                select new { sc.Id, sc.Name };

            var courses = await coursesQuery.Select(c=> new DropDownDTO { Id = c.Id, Name = c.Name}).ToListAsync();

            var roomsQuery = from cr in context.ClassRooms
                             join ctc in context.ClassRoomTeacherCourses
                             on cr.Id equals ctc.ClassRoomId into sc_ctc
                             from ctc in sc_ctc.DefaultIfEmpty()
                             where cr.SchoolId == schoolId && ctc == null
                             select new { cr.Id, cr.Name }; 

            var rooms = await roomsQuery.Select(r=> new DropDownDTO { Id = r.Id, Name = r.Name }).ToListAsync();

            var result = new ClassRoomTeacherCourseInitialDTO
            {
                ClassRooms = rooms,
                Courses = courses
            };

            return result;


        }

        public async Task<IEnumerable<ClassRoomTeacherCourseResultDTO>> GetTeacherDataAsync(Guid teacherId)
        {
            var data = await context.ClassRoomTeacherCourses
                            .Where(c=> c.TeacherId == teacherId)
                            .Select(c=> new ClassRoomTeacherCourseResultDTO
                            {
                                TeacherId = c.TeacherId,
                                TeacherName = $"{c.Teacher.FirstName} {c.Teacher.MiddelName} {c.Teacher.LastName}",
                                ClassRoomId = c.ClassRoomId,
                                ClassRoomName = c.ClassRoom.Name,
                                CourseId = c.CourseId,
                                CourseName = c.Course.Name
                            }).ToListAsync();

            return data;
        }

        public async Task<ClassRoomTeacherCourseResultDTO?> GetClassRoomIeacherCourseAsync(Guid teacherId, Guid roomId, Guid courseId)
        {
            var row = await context.ClassRoomTeacherCourses.FirstOrDefaultAsync(c => c.TeacherId == teacherId &&
                                                                                    c.ClassRoomId == roomId &&
                                                                                    c.CourseId == courseId);

            if(row == null)
                return null;

            return new ClassRoomTeacherCourseResultDTO
            {
                TeacherId = row.TeacherId,
                TeacherName = $"{row.Teacher.FirstName} {row.Teacher.MiddelName} {row.Teacher.LastName}",
                ClassRoomId = row.ClassRoomId,
                ClassRoomName = row.ClassRoom.Name,
                CourseId = row.CourseId,
                CourseName = row.Course.Name
            };
        }

        public async Task<ClassRoomTeacherCourse?> GetByIdAsync(Guid teacherId, Guid roomId, Guid courseId)
        {
            return await context.ClassRoomTeacherCourses.FirstOrDefaultAsync(c => c.TeacherId == teacherId &&
                                                                                    c.ClassRoomId == roomId &&
                                                                                    c.CourseId == courseId);

            
        }





    }
}
