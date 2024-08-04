using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Helpers;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.StudentManagement.Query
{
    public class StudentQueryRepository : BaseQueryRepository<Student>, IStudentQueryRepository
    {
        private readonly AppDbContext context;

        public StudentQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<StudentResultDTO>> GetStudentsBySchoolIdAsync(Guid schoolId)
        {
            return await context.Students.Where(s=> s.SchoolId == schoolId).Select(s => new StudentResultDTO
            {
                Id = s.Id,
                Active = s.Active,
                BirthDate = s.BirthDate,
                ClassRoomId = s.ClassRoomId,
                ClassRoomName = s.ClassRoom.Name,
                Code = s.Code,
                FirstName = s.FirstName,
                LastName = s.LastName,
                MiddelName = s.MiddelName,
                SchoolId = schoolId,
                SchoolName = s.School.Name,
                ImageUrl = !string.IsNullOrEmpty(s.ImageUrl) ? $"{SettingsHelper.GetAppUrl()}/{s.ImageUrl.Replace("\\", "/")}" : string.Empty,
                IsPayed = s.IsPayed
            })
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentResultDTO>> GetStudentsBySchoolIdAndClassRoomIdAsync(Guid schoolId, Guid classroomId)
        {
            return await context.Students.Where(s => s.SchoolId == schoolId && s.ClassRoomId == classroomId)
                .Select(s => new StudentResultDTO
                {
                    Id = s.Id,
                    Active = s.Active,
                    BirthDate = s.BirthDate,
                    ClassRoomId = s.ClassRoomId,
                    ClassRoomName = s.ClassRoom.Name,
                    Code = s.Code,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    MiddelName = s.MiddelName,
                    SchoolId = schoolId,
                    SchoolName = s.School.Name,
                    ImageUrl = !string.IsNullOrEmpty(s.ImageUrl) ? $"{SettingsHelper.GetAppUrl()}/{s.ImageUrl.Replace("\\", "/")}" : string.Empty,
                    IsPayed = s.IsPayed
                })
            .ToListAsync();
        }

        public async Task<StudentResultDTO?> GetStudentByIdAsync(Guid studentId)
        {
            return await context.Students
                .Select(s => new StudentResultDTO
                {
                    Id = s.Id,
                    Active = s.Active,
                    BirthDate = s.BirthDate,
                    ClassRoomId = s.ClassRoomId,
                    ClassRoomName = s.ClassRoom.Name,
                    Code = s.Code,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    MiddelName = s.MiddelName,
                    SchoolId = s.SchoolId,
                    SchoolName = s.School.Name,
                    ImageUrl = !string.IsNullOrEmpty(s.ImageUrl) ? $"{SettingsHelper.GetAppUrl()}/{s.ImageUrl.Replace("\\", "/")}" : string.Empty,
                    IsPayed = s.IsPayed
                }).FirstOrDefaultAsync(s => s.Id == studentId);
        }

        public async Task<string> GetMaxStudebtCodeAsync(Guid schoolId)
        {
            var anyStudents = await context.Students.AnyAsync(s => s.SchoolId == schoolId);
            if(!anyStudents)
            {
                return null;
            }
            var student = await context.Students.LastOrDefaultAsync(s => s.SchoolId == schoolId);
            return student.Code;
        }
    }
}
