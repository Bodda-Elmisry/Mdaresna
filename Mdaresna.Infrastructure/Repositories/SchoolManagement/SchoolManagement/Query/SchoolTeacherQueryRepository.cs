using Azure.Core;
using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Helpers;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public async Task<bool> isExist(Guid schoolId, Guid teacherId)
        {
            return await context.schoolTeachers.AnyAsync(s => s.SchoolId == schoolId && s.TeacherId == teacherId);
        }

        public async Task<SchoolTeacher> GetSchoolTeacherByIdAsync(Guid schoolId, Guid teacherId)
        {
            return await context.schoolTeachers.FirstOrDefaultAsync(s=> s.SchoolId == schoolId && s.TeacherId ==  teacherId);
        }

        public async Task<IEnumerable<TeacherResultDTO>> GetSchoolTeachersAsync(Guid schoolId, string teacherName, string teacherPhoneNumber, string teacherEmail)
        {
            //var teachers = await context.schoolTeachers
            //    .Where(s=> s.SchoolId == schoolId)
            //    .Select(t => new TeacherResultDTO
            //{
            //    Id = t.Teacher.Id,
            //    UserName = t.Teacher.UserName,
            //    FirstName = t.Teacher.FirstName,
            //    MiddelName = t.Teacher.MiddelName,
            //    LastName = t.Teacher.LastName,
            //    PhoneNumber = t.Teacher.PhoneNumber,
            //    BirthDay = t.Teacher.BirthDay,
            //    Email = t.Teacher.Email,
            //    Address = t.Teacher.Address,
            //    City = t.Teacher.City,
            //    Region = t.Teacher.Region,
            //    Contry = t.Teacher.Contry,
            //    ImageUrl = t.Teacher.ImageUrl
            //}).ToListAsync();

            //var teacherQuery = from st in context.schoolTeachers
            //                   join stc in (
            //                   from stc in context.schoolTeacherCourses
            //                   group stc by new { stc.SchoolId, stc.TeacherId } into g
            //                   select new
            //                   {
            //                       SchoolId = g == null ? (Guid?) g.Key.SchoolId : null,
            //                       TeacherId = g == null ? (Guid?) g.Key.TeacherId : null,
            //                       TeacherCoursesCount = (int?)g.Count()
            //                   }
            //                   )
            //                   on new { st.SchoolId, st.TeacherId } equals new { stc.SchoolId, stc.TeacherId } into stcGroup
            //                   from stc in stcGroup.DefaultIfEmpty()
            //                   select new 
            //                   {
            //                       Id = st.Teacher.Id,
            //                       UserName = st.Teacher.UserName,
            //                       FirstName = st.Teacher.FirstName,
            //                       MiddelName = st.Teacher.MiddelName,
            //                       LastName = st.Teacher.LastName,
            //                       PhoneNumber = st.Teacher.PhoneNumber,
            //                       BirthDay = st.Teacher.BirthDay,
            //                       Email = st.Teacher.Email,
            //                       Address = st.Teacher.Address,
            //                       City = st.Teacher.City,
            //                       Region = st.Teacher.Region,
            //                       Contry = st.Teacher.Contry,
            //                       ImageUrl = st.Teacher.ImageUrl,
            //                       CoursesCount = stc == null ? 0 : stc.TeacherCoursesCount == null ? 0 : stc.TeacherCoursesCount
            //                   };
            var teacherQuery = from st in context.schoolTeachers.Where(s=> s.SchoolId == schoolId)
                               join stcGroup in (
                                   from stc in context.schoolTeacherCourses
                                   group stc by new { stc.SchoolId, stc.TeacherId } into g
                                   select new
                                   {
                                       SchoolId = g.Key.SchoolId,
                                       TeacherId = g.Key.TeacherId,
                                       TeacherCoursesCount = (int?)g.Count()
                                   }
                               )
                               on new { st.SchoolId, st.TeacherId } equals new { stcGroup.SchoolId, stcGroup.TeacherId } into stcGroupJoin
                               from stc in stcGroupJoin.DefaultIfEmpty()
                               select new TeacherResultDTO
                               {
                                   Id = st.Teacher.Id,
                                   UserName = st.Teacher.UserName,
                                   FirstName = st.Teacher.FirstName,
                                   MiddelName = st.Teacher.MiddelName,
                                   LastName = st.Teacher.LastName,
                                   PhoneNumber = st.Teacher.PhoneNumber,
                                   BirthDay = st.Teacher.BirthDay,
                                   Email = st.Teacher.Email,
                                   Address = st.Teacher.Address,
                                   City = st.Teacher.City,
                                   Region = st.Teacher.Region,
                                   Contry = st.Teacher.Contry,
                                   ImageUrl = !string.IsNullOrEmpty(st.Teacher.ImageUrl) ? $"{SettingsHelper.GetAppUrl()}/{st.Teacher.ImageUrl.Replace("\\","/")}" : null,
                                   CoursesCount = stc.TeacherCoursesCount == null ? 0 : stc.TeacherCoursesCount
                               };

            teacherQuery = !string.IsNullOrEmpty(teacherEmail) ? teacherQuery.Where(t => t.Email.Contains(teacherEmail)) : teacherQuery;
            teacherQuery = !string.IsNullOrEmpty(teacherPhoneNumber) ? teacherQuery.Where(t => t.PhoneNumber.Contains(teacherPhoneNumber)) : teacherQuery;
            teacherQuery = !string.IsNullOrEmpty(teacherName) ? teacherQuery.Where(t => (t.FirstName + t.MiddelName + t.LastName).Contains(teacherName.Replace(" ", ""))) : teacherQuery;



            var query = teacherQuery.ToQueryString();
            var teachers = await teacherQuery.ToListAsync();
            return teachers;
                
        }

        private int GetCount(int? coursestCount)
        {
            var result = coursestCount == null ? 0 : coursestCount.Value;
            return result;
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
