using Mdaresna.Doamin.DTOs.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
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
