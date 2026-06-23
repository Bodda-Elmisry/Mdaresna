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

        public async Task<IEnumerable<ClassRoomResultDTO>> GetBySchoolIdFilteredAsync(GetSchoolClassesFilteredDTO filterDto)
        {
            var query = context.ClassRooms.Where(c => c.SchoolId == filterDto.SchoolId && c.Deleted == false);

            if (!string.IsNullOrEmpty(filterDto.Name))
            {
                query = query.Where(c => c.Name.Contains(filterDto.Name));
            }

            if (filterDto.LanguageId.HasValue && filterDto.LanguageId.Value != Guid.Empty)
            {
                query = query.Where(c => c.LanguageId == filterDto.LanguageId.Value);
            }

            if (filterDto.GradeId.HasValue && filterDto.GradeId.Value != Guid.Empty)
            {
                query = query.Where(c => c.GradeId == filterDto.GradeId.Value);
            }

            if (filterDto.Gender.HasValue)
            {
                query = query.Where(c => c.Gender == filterDto.Gender.Value);
            }

            return await query.Select(c => new ClassRoomResultDTO
            {
                Id = c.Id,
                Name = c.Name,
                maxOfStudents = c.maxOfStudents,
                SupervisorId = c.SupervisorId,
                SupervisorName = c.Supervisor != null ? $"{c.Supervisor.FirstName} {c.Supervisor.MiddelName} {c.Supervisor.LastName}" : string.Empty,
                Active = c.Active,
                WCSUrl = c.WCSUrl,
                SchoolId = c.SchoolId,
                SchoolName = c.School != null ? c.School.Name : string.Empty,
                LanguageId = c.LanguageId,
                LanguageName = c.Language != null ? c.Language.Name : string.Empty,
                GradeId = c.GradeId,
                Gradename = c.Grade != null ? c.Grade.Name : string.Empty,
                Gender = c.Gender
            }).ToListAsync();
        }

        public async Task<IEnumerable<ClassRoomResultDTO>> GetBySchoolIdAsync(Guid SchoolId)
        {
            return await context.ClassRooms
                            //.Include(c=> c.Grade).Include(c=> c.Language).Include(c=> c.Supervisor)
                            .Where(c=> c.SchoolId == SchoolId && c.Deleted == false)
                            .Select(c=> new ClassRoomResultDTO
                            {
                                Id = c.Id,
                                Name = c.Name,
                                maxOfStudents = c.maxOfStudents,
                                SupervisorId = c.SupervisorId,
                                SupervisorName = c.Supervisor != null ? $"{c.Supervisor.FirstName} {c.Supervisor.MiddelName} {c.Supervisor.LastName}" : string.Empty,
                                Active = c.Active,
                                WCSUrl = c.WCSUrl,
                                SchoolId = c.SchoolId,
                                SchoolName = c.School != null ? c.School.Name : string.Empty,
                                LanguageId = c.LanguageId,
                                LanguageName = c.Language != null ? c.Language.Name : string.Empty,
                                GradeId = c.GradeId,
                                Gradename = c.Grade != null ? c.Grade.Name : string.Empty,
                                Gender = c.Gender

                            })
                            .ToListAsync();
        }

        public async Task<IEnumerable<ClassRoom>> GetCLassroomsBySchoolIdAsync(Guid SchoolId)
        {
            return await context.ClassRooms
                            //.Include(c=> c.Grade).Include(c=> c.Language).Include(c=> c.Supervisor)
                            .Where(c => c.SchoolId == SchoolId && c.Deleted == false).ToListAsync();
        }

        public async Task<IEnumerable<ClassRoomResultDTO>> GetBySchoolIdAndSupervisorIdAsync(Guid SchoolId, Guid SupervisorId)
        {
            return await context.ClassRooms
                            .Where(c => c.SchoolId == SchoolId && c.SupervisorId == SupervisorId && c.Deleted == false)
                            .Select(c => new ClassRoomResultDTO
                            {
                                Id = c.Id,
                                Name = c.Name,
                                maxOfStudents = c.maxOfStudents,
                                SupervisorId = c.SupervisorId,
                                SupervisorName = c.Supervisor != null ? $"{c.Supervisor.FirstName} {c.Supervisor.MiddelName} {c.Supervisor.LastName}" : string.Empty,
                                Active = c.Active,
                                WCSUrl = c.WCSUrl,
                                SchoolId = c.SchoolId,
                                SchoolName = c.School != null ? c.School.Name : string.Empty,
                                LanguageId = c.LanguageId,
                                LanguageName = c.Language != null ? c.Language.Name : string.Empty,
                                GradeId = c.GradeId,
                                Gradename = c.Grade != null ? c.Grade.Name : string.Empty,
                                Gender = c.Gender

                            }).ToListAsync();
        }

        public async Task<IEnumerable<ClassRoomResultDTO>> GetBySchoolIdAndUserIdAsync(Guid schoolId, Guid userId)
        {
            try
            {

                var tquery = from cr in context.ClassRooms
                             join s in context.Schools on cr.SchoolId equals s.Id
                             join su in context.Users on cr.SupervisorId equals su.Id into supervisorUsers
                             from su in supervisorUsers.DefaultIfEmpty()
                             join l in context.Languages on cr.LanguageId equals l.Id
                             join g in context.SchoolGrades on cr.GradeId equals g.Id
                             join crtc in context.ClassRoomTeacherCourses.Where(tc => !tc.Deleted) on cr.Id equals crtc.ClassRoomId into classroomTeacherCourses
                             from crtc in classroomTeacherCourses.DefaultIfEmpty()
                             where (s.SchoolAdminId == userId || cr.SupervisorId == userId || crtc.TeacherId == userId)
                                   && s.Id == schoolId
                                   && cr.Active == true
                                   && cr.Deleted == false
                             select new { cr, s, su, l, g };

                var equery = from cr in context.ClassRooms
                             join s in context.Schools on cr.SchoolId equals s.Id
                             join su in context.Users on cr.SupervisorId equals su.Id into supervisorUsers
                             from su in supervisorUsers.DefaultIfEmpty()
                             join l in context.Languages on cr.LanguageId equals l.Id
                             join g in context.SchoolGrades on cr.GradeId equals g.Id
                             join crtc in context.ClassroomEmployees.Where(e => !e.Deleted) on cr.Id equals crtc.ClassRoomId into classroomEmployees
                             from crtc in classroomEmployees.DefaultIfEmpty()
                             where (s.SchoolAdminId == userId || cr.SupervisorId == userId || crtc.EmployeeId == userId)
                                   && s.Id == schoolId
                                   && cr.Active == true
                                   && cr.Deleted == false
                             select new { cr, s, su, l, g };

                var query = tquery.Union(equery).Distinct(); // ✅ Union before projection

                var queryString = query.ToQueryString(); // For debugging

                Console.WriteLine(queryString);

                var result = await query
                    .Select(x => new ClassRoomResultDTO
                    {
                        Id = x.cr.Id,
                        Name = x.cr.Name,
                        maxOfStudents = x.cr.maxOfStudents,
                        SupervisorId = x.cr.SupervisorId,
                        SupervisorName = x.su != null ? x.su.FirstName + " " + x.su.LastName : string.Empty,
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
            var room = await context.ClassRooms.FirstOrDefaultAsync(c => c.Id == roomId && c.Deleted == false);

            return room == null ? null :
                            new ClassRoomResultDTO
                            {
                                Id = room.Id,
                                Name = room.Name,
                                maxOfStudents = room.maxOfStudents,
                                SupervisorId = room.SupervisorId,
                                SupervisorName = room.Supervisor != null ? $"{room.Supervisor.FirstName} {room.Supervisor.MiddelName} {room.Supervisor.LastName}" : string.Empty,
                                Active = room.Active,
                                WCSUrl = room.WCSUrl,
                                SchoolId = room.SchoolId,
                                SchoolName = room.School != null ? room.School.Name : string.Empty,
                                LanguageId = room.LanguageId,
                                LanguageName = room.Language != null ? room.Language.Name : string.Empty,
                                GradeId = room.GradeId,
                                Gradename = room.Grade != null ? room.Grade.Name : string.Empty,
                                Gender = room.Gender

                            };
        }

    }
}
