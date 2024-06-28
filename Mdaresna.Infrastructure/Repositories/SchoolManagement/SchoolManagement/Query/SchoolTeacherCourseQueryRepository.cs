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

        public async Task<IEnumerable<SchoolCourse>> GetSchoolTeacherCourcesAsync(Guid schoolId, Guid teacherId)
        {
            return await context.schoolTeacherCourses
                        .Where(s => s.TeacherId == teacherId && s.SchoolId == schoolId)
                        .Select(s => s.Course).ToListAsync();

        }

        public async Task<SchoolTeacherCourse?> GetSchoolTeacherCourceAsync(Guid schoolId, Guid teacherId, Guid courseId)
        {
            return await context.schoolTeacherCourses
                        .FirstOrDefaultAsync(s => s.TeacherId == teacherId && s.SchoolId == schoolId && s.CourseId == courseId);

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
