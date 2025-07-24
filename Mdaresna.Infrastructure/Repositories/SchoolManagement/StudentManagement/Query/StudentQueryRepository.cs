using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Helpers;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
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
    public class StudentQueryRepository : BaseQueryRepository<Student>, IStudentQueryRepository
    {
        private readonly AppDbContext context;

        public StudentQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<StudentResultDTO>> GetStudentsBySchoolIdViewAsync(Guid schoolId, string studentCode, string studentName)
        {
            var query = context.Students.Where(s => s.SchoolId == schoolId && s.Deleted == false);

            if (!string.IsNullOrEmpty(studentCode))
            {
                query = query.Where(q => q.Code.Contains(studentCode));
            }

            if (!string.IsNullOrEmpty(studentName))
            {
                query = query.Where(q => EF.Functions.Like(
    q.FirstName + " " + q.MiddelName + " " + q.LastName,
    $"%{studentName}%"
));
            }


            Console.WriteLine(query.ToQueryString());

            var result = query.Select(s => new StudentResultDTO
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
            }).OrderByDescending(s => s.BirthDate)
                .ToListAsync();


            return await result;
        }

        public async Task<IEnumerable<Student>> GetStudentsBySchoolIdAsync(Guid schoolId, bool? ispayed)
        {
            var query = context.Students.Where(s => s.SchoolId == schoolId && s.Deleted == false && s.Active == true);

            query = ispayed.HasValue ? query.Where(s => s.IsPayed == ispayed.Value) : query;

            var result = query.ToListAsync();


            return await result;
        }

        public async Task<IEnumerable<StudentResultDTO>> GetStudentsBySchoolIdAndClassRoomIdAsync(Guid schoolId, Guid classroomId)
        {
            return await context.Students.Where(s => s.SchoolId == schoolId && s.ClassRoomId == classroomId && s.Deleted == false)
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
            var student = await context.Students
                                .Include(s => s.ClassRoom).Include(s => s.School)
                                .FirstOrDefaultAsync(s => s.Id == studentId && s.Deleted == false);
            return student == null ? null :
                new StudentResultDTO
                {
                    Id = student.Id,
                    Active = student.Active,
                    BirthDate = student.BirthDate,
                    ClassRoomId = student.ClassRoomId,
                    ClassRoomName = student.ClassRoom.Name,
                    Code = student.Code,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    MiddelName = student.MiddelName,
                    SchoolId = student.SchoolId,
                    SchoolName = student.School.Name,
                    ImageUrl = !string.IsNullOrEmpty(student.ImageUrl) ? $"{SettingsHelper.GetAppUrl()}/{student.ImageUrl.Replace("\\", "/")}" : string.Empty,
                    IsPayed = student.IsPayed
                };
        }

        public async Task<StudentResultDTO?> GetStudentByCodeAsync(string code)
        {
            var student = await context.Students
                                .Include(s => s.ClassRoom).Include(s => s.School)
                                .FirstOrDefaultAsync(s => s.Code == code && s.Deleted == false);
            return student == null ? null :
                new StudentResultDTO
                {
                    Id = student.Id,
                    Active = student.Active,
                    BirthDate = student.BirthDate,
                    ClassRoomId = student.ClassRoomId,
                    ClassRoomName = student.ClassRoom.Name,
                    Code = student.Code,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    MiddelName = student.MiddelName,
                    SchoolId = student.SchoolId,
                    SchoolName = student.School.Name,
                    ImageUrl = !string.IsNullOrEmpty(student.ImageUrl) ? $"{SettingsHelper.GetAppUrl()}/{student.ImageUrl.Replace("\\", "/")}" : string.Empty,
                    IsPayed = student.IsPayed
                };
        }

        public async Task<string> GetMaxStudebtCodeAsync(Guid schoolId)
        {
            var anyStudents = await context.Students.AnyAsync(s => s.SchoolId == schoolId && s.Deleted == false);
            if (!anyStudents)
            {
                return null;
            }
            var student = await context.Students.OrderByDescending(s => s.CreateDate).FirstOrDefaultAsync(s => s.SchoolId == schoolId && s.Deleted == false);

            return student != null ? student.Code : string.Empty;
        }
    }
}
