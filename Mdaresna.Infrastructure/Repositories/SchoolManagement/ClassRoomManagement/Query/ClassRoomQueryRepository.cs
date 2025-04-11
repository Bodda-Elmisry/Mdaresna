using Mdaresna.Doamin.DTOs.ClassRoomManagement;
using Mdaresna.Doamin.Models.AdminManagement;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.ClassRoomManagement.Query
{
    public class ClassRoomQueryRepository : BaseQueryRepository<ClassRoom>, IClassRoomQueryRepository
    {
        private readonly AppDbContext context;

        public ClassRoomQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<ClassRoomResultDTO>> GetBySchoolIdAsync(Guid SchoolId)
        {
            return await context.ClassRooms
                            //.Include(c=> c.Grade).Include(c=> c.Language).Include(c=> c.Supervisor)
                            .Where(c=> c.SchoolId == SchoolId)
                            .Select(c=> new ClassRoomResultDTO
                            {
                                Id = c.Id,
                                Name = c.Name,
                                maxOfStudents = c.maxOfStudents,
                                SupervisorId = c.SupervisorId,
                                SupervisorName = $"{c.Supervisor.FirstName} {c.Supervisor.MiddelName} {c.Supervisor.LastName}",
                                Active = c.Active,
                                WCSUrl = c.WCSUrl,
                                SchoolId = c.SchoolId,
                                SchoolName = c.School.Name,
                                LanguageId = c.LanguageId,
                                LanguageName = c.Language.Name,
                                GradeId = c.GradeId,
                                Gradename = c.Grade.Name,
                                Gender = c.Gender

                            })
                            .ToListAsync();
        }

        public async Task<IEnumerable<ClassRoomResultDTO>> GetBySchoolIdAndSupervisorIdAsync(Guid SchoolId, Guid SupervisorId)
        {
            return await context.ClassRooms
                            .Where(c => c.SchoolId == SchoolId && c.SupervisorId == SupervisorId)
                            .Select(c => new ClassRoomResultDTO
                            {
                                Id = c.Id,
                                Name = c.Name,
                                maxOfStudents = c.maxOfStudents,
                                SupervisorId = c.SupervisorId,
                                SupervisorName = $"{c.Supervisor.FirstName} {c.Supervisor.MiddelName} {c.Supervisor.LastName}",
                                Active = c.Active,
                                WCSUrl = c.WCSUrl,
                                SchoolId = c.SchoolId,
                                SchoolName = c.School.Name,
                                LanguageId = c.LanguageId,
                                LanguageName = c.Language.Name,
                                GradeId = c.GradeId,
                                Gradename = c.Grade.Name,
                                Gender = c.Gender

                            }).ToListAsync();
        }

        public async Task<IEnumerable<ClassRoomResultDTO>> GetBySchoolIdAndUserIdAsync(Guid schoolId, Guid userId)
        {
            try
            {
                //var tquery = (from cr in context.ClassRooms
                //              join s in context.Schools on cr.SchoolId equals s.Id
                //              join su in context.Users on cr.SupervisorId equals su.Id
                //              join l in context.Languages on cr.LanguageId equals l.Id
                //              join g in context.SchoolGrades on cr.GradeId equals g.Id
                //              join crtc in context.ClassRoomTeacherCourses
                //                  on cr.Id equals crtc.ClassRoomId into classroomTeacherCourses
                //              from crtc in classroomTeacherCourses.DefaultIfEmpty()
                //              where (s.SchoolAdminId == userId || cr.SupervisorId == userId || crtc.TeacherId == userId)
                //                    && s.Id == schoolId
                //                    && cr.Active == true
                //              orderby cr.Name
                //              select new
                //              {
                //                  Id = cr.Id,
                //                  Name = cr.Name,
                //                  maxOfStudents = cr.maxOfStudents,
                //                  SupervisorId = cr.SupervisorId,
                //                  SupervisorName = $"{su.FirstName} {su.LastName}",
                //                  Active = cr.Active,
                //                  WCSUrl = cr.WCSUrl,
                //                  SchoolId = cr.SchoolId,
                //                  SchoolName = s.Name,
                //                  LanguageId = cr.LanguageId,
                //                  LanguageName = l.Name,
                //                  GradeId = cr.GradeId,
                //                  Gradename = g.Name,
                //                  Gender = cr.Gender

                //              })
                //             .Distinct();

                //var equery = (from cr in context.ClassRooms
                //              join s in context.Schools on cr.SchoolId equals s.Id
                //              join su in context.Users on cr.SupervisorId equals su.Id
                //              join l in context.Languages on cr.LanguageId equals l.Id
                //              join g in context.SchoolGrades on cr.GradeId equals g.Id
                //              join crtc in context.ClassroomEmployees
                //                  on cr.Id equals crtc.ClassRoomId into ClassroomEmployees
                //              from crtc in ClassroomEmployees.DefaultIfEmpty()
                //              where (s.SchoolAdminId == userId || cr.SupervisorId == userId || crtc.EmployeeId == userId)
                //                    && s.Id == schoolId
                //                    && cr.Active == true
                //              orderby cr.Name
                //              select new
                //              {
                //                  Id = cr.Id,
                //                  Name = cr.Name,
                //                  maxOfStudents = cr.maxOfStudents,
                //                  SupervisorId = cr.SupervisorId,
                //                  SupervisorName = $"{su.FirstName} {su.LastName}",
                //                  Active = cr.Active,
                //                  WCSUrl = cr.WCSUrl,
                //                  SchoolId = cr.SchoolId,
                //                  SchoolName = s.Name,
                //                  LanguageId = cr.LanguageId,
                //                  LanguageName = l.Name,
                //                  GradeId = cr.GradeId,
                //                  Gradename = g.Name,
                //                  Gender = cr.Gender

                //              })
                //             .Distinct();

                //var query = tquery.Union(equery).Distinct();

                //var queryString = query.ToQueryString();

                //var result = await query.ToListAsync();

                //return result.Select(cr => new ClassRoomResultDTO
                //{
                //    Id = cr.Id,
                //    Name = cr.Name,
                //    maxOfStudents = cr.maxOfStudents,
                //    SupervisorId = cr.SupervisorId,
                //    SupervisorName = cr.SupervisorName,
                //    Active = cr.Active,
                //    WCSUrl = cr.WCSUrl,
                //    SchoolId = cr.SchoolId,
                //    SchoolName = cr.SchoolName,
                //    LanguageId = cr.LanguageId,
                //    LanguageName = cr.LanguageName,
                //    GradeId = cr.GradeId,
                //    Gradename = cr.Gradename,
                //    Gender = cr.Gender

                //});

                var tquery = from cr in context.ClassRooms
                             join s in context.Schools on cr.SchoolId equals s.Id
                             join su in context.Users on cr.SupervisorId equals su.Id
                             join l in context.Languages on cr.LanguageId equals l.Id
                             join g in context.SchoolGrades on cr.GradeId equals g.Id
                             join crtc in context.ClassRoomTeacherCourses on cr.Id equals crtc.ClassRoomId into classroomTeacherCourses
                             from crtc in classroomTeacherCourses.DefaultIfEmpty()
                             where (s.SchoolAdminId == userId || cr.SupervisorId == userId || crtc.TeacherId == userId)
                                   && s.Id == schoolId
                                   && cr.Active == true
                             select new { cr, s, su, l, g };

                var equery = from cr in context.ClassRooms
                             join s in context.Schools on cr.SchoolId equals s.Id
                             join su in context.Users on cr.SupervisorId equals su.Id
                             join l in context.Languages on cr.LanguageId equals l.Id
                             join g in context.SchoolGrades on cr.GradeId equals g.Id
                             join crtc in context.ClassroomEmployees on cr.Id equals crtc.ClassRoomId into classroomEmployees
                             from crtc in classroomEmployees.DefaultIfEmpty()
                             where (s.SchoolAdminId == userId || cr.SupervisorId == userId || crtc.EmployeeId == userId)
                                   && s.Id == schoolId
                                   && cr.Active == true
                             select new { cr, s, su, l, g };

                var query = tquery.Union(equery).Distinct(); // ✅ Union before projection

                var queryString = query.ToQueryString(); // For debugging

                var result = await query
                    .Select(x => new ClassRoomResultDTO
                    {
                        Id = x.cr.Id,
                        Name = x.cr.Name,
                        maxOfStudents = x.cr.maxOfStudents,
                        SupervisorId = x.cr.SupervisorId,
                        SupervisorName = x.su.FirstName + " " + x.su.LastName,
                        Active = x.cr.Active,
                        WCSUrl = x.cr.WCSUrl,
                        SchoolId = x.cr.SchoolId,
                        SchoolName = x.s.Name,
                        LanguageId = x.cr.LanguageId,
                        LanguageName = x.l.Name,
                        GradeId = x.cr.GradeId,
                        Gradename = x.g.Name,
                        Gender = x.cr.Gender
                    })
                    .ToListAsync();

                return result;
            }
            catch(Exception ex)
            {
                throw new Exception("Error in GetBySchoolIdAndUserIdAsync: " + ex.Message);
            }
        }


        public async Task<ClassRoomResultDTO?> GetClassRoomByIdAsync(Guid roomId)
        {
            var room = await context.ClassRooms.FirstOrDefaultAsync(c => c.Id == roomId);

            return room == null ? null :
                            new ClassRoomResultDTO
                            {
                                Id = room.Id,
                                Name = room.Name,
                                maxOfStudents = room.maxOfStudents,
                                SupervisorId = room.SupervisorId,
                                SupervisorName = $"{room.Supervisor.FirstName} {room.Supervisor.MiddelName} {room.Supervisor.LastName}",
                                Active = room.Active,
                                WCSUrl = room.WCSUrl,
                                SchoolId = room.SchoolId,
                                SchoolName = room.School.Name,
                                LanguageId = room.LanguageId,
                                LanguageName = room.Language.Name,
                                GradeId = room.GradeId,
                                Gradename = room.Grade.Name,
                                Gender = room.Gender

                            };
        }

    }
}
