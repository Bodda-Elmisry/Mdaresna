using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Query
{
    public class SchoolTeacherCourseQueryRepository : BaseQueryRepository<SchoolTeacherCourse>, ISchoolTeacherCourseQueryRepository
    {
        private readonly AppDbContext context;

        public SchoolTeacherCourseQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<SchoolTeacherCourseResultDTO>> GetSchoolTeacherCourcesAsync(Guid schoolId, Guid teacherId)
        {
            return await context.schoolTeacherCourses
                        .Select(s => new SchoolTeacherCourseResultDTO
                        {
                            TeacherId = s.TeacherId,
                            TeacherName = $"{s.Teacher.FirstName} {s.Teacher.MiddelName} {s.Teacher.LastName}",
                            CourseId = s.CourseId,
                            CourseName = s.Course.Name,
                            SchoolId = s.SchoolId,
                            SchoolName = s.School.Name

                        })
                        .Where(s => s.TeacherId == teacherId && s.SchoolId == schoolId).ToListAsync();
                        //.Select(s => s.Course).ToListAsync();

        }

        public async Task<SchoolTeacherCourseResultDTO?> GetSchoolTeacherCourceAsync(Guid schoolId, Guid teacherId, Guid courseId)
        {
            var model =  await context.schoolTeacherCourses
                        .FirstOrDefaultAsync(s => s.TeacherId == teacherId && s.SchoolId == schoolId && s.CourseId == courseId);

            return model != null ? new SchoolTeacherCourseResultDTO
            {
                TeacherId = model.TeacherId,
                TeacherName = $"{model.Teacher.FirstName} {model.Teacher.MiddelName} {model.Teacher.LastName}",
                CourseId = model.CourseId,
                CourseName = model.Course.Name,
                SchoolId = model.SchoolId,
                SchoolName = model.School.Name
            } : null;

        }

        public async Task<SchoolTeacherCourseInitialDTO> GetSchoolTeacherCourceInitialListsAsync(Guid schoolId)
        {
            var teachers = await context.schoolTeacherCourses.Where(s => s.SchoolId == schoolId)
                .Select(s => new DropDownDTO
                {
                    Id = s.Teacher.Id,
                    Name = string.Format("{0} {1} {2}", s.Teacher.FirstName, s.Teacher.MiddelName, s.Teacher.LastName),
                }).ToListAsync();

            var courses = await context.schoolTeacherCourses.Where(s => s.SchoolId == schoolId)
                .Select(s => new DropDownDTO
                {
                    Id = s.Course.Id,
                    Name = s.Course.Name,
                }).ToListAsync();

            var result = new SchoolTeacherCourseInitialDTO
            {
                SchoolTeachers = teachers,
                SchoolCourses = courses
            };

            return result;

        }

    }
}
