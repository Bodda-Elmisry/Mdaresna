using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Query
{
    public class SchoolTeacherQueryRepository : BaseQueryRepository<SchoolTeacher>, ISchoolTeacherQueryRepository
    {
        private readonly AppDbContext context;

        public SchoolTeacherQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<SchoolTeacher> GetSchoolTeacherByIdAsync(Guid schoolId, Guid teacherId)
        {
            return await context.schoolTeachers.FirstOrDefaultAsync(s=> s.SchoolId == schoolId && s.TeacherId ==  teacherId);
        }

        public async Task<IEnumerable<TeacherResultDTO>> GetSchoolTeachersAsync(Guid schoolId)
        {
            var teachers = await context.schoolTeachers
                .Where(s=> s.SchoolId == schoolId)
                .Select(t => new TeacherResultDTO
            {
                Id = t.Teacher.Id,
                UserName = t.Teacher.UserName,
                FirstName = t.Teacher.FirstName,
                MiddelName = t.Teacher.MiddelName,
                LastName = t.Teacher.LastName,
                PhoneNumber = t.Teacher.PhoneNumber,
                BirthDay = t.Teacher.BirthDay,
                Email = t.Teacher.Email,
                Address = t.Teacher.Address,
                City = t.Teacher.City,
                Region = t.Teacher.Region,
                Contry = t.Teacher.Contry,
                ImageUrl = t.Teacher.ImageUrl
            }).ToListAsync();

            return teachers;
                
        }

        public async Task<IEnumerable<TeacherSchoolResultDTO>> GetTeacherSchoolsAsync(Guid teacherId)
        {
            var schools = await context.schoolTeachers
                .Where(s => s.TeacherId == teacherId)
                .Select(s => new TeacherSchoolResultDTO
                {
                    Id = s.School.Id,
                    Name = s.School.Name,
                    ImageUrl = s.School.ImageUrl
                }).ToListAsync();


            return schools;
        }

    }
}
