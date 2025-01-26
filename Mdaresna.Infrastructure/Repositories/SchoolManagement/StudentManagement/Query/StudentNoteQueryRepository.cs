using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.DTOs.StudentManagement;
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
    public class StudentNoteQueryRepository : BaseQueryRepository<StudentNote>, IStudentNoteQueryRepository
    {
        private readonly AppDbContext context;
        private readonly AppSettingDTO appSettings;

        public StudentNoteQueryRepository(AppDbContext context,
                                                IOptions<AppSettingDTO> appSettings) : base(context)
        {
            this.context = context;
            this.appSettings = appSettings.Value;
        }

        public async Task<IEnumerable<StudentNoteResultDTO>> GetStudentNotesListAsync(Guid? StudentId, Guid? ClassRoomId, Guid? SupervisorId, Guid? CourseId, DateTime? DateFrom, DateTime? DateTo, string? Notes, int pageNumber)
        {
            int pagesize = appSettings.PageSize != null ? appSettings.PageSize.Value : 30;
            var query = context.studentNotes.AsQueryable();

            if(StudentId != null )
                query = query.Where(n => n.StudentId == StudentId);

            if (ClassRoomId != null)
                query = query.Where(n=> n.ClassRoomId == ClassRoomId.Value);

            if (SupervisorId != null)
                query = query.Where(n => n.SupervisorId == SupervisorId.Value);

            if (CourseId != null)
                query = query.Where(n => n.CourseId == CourseId.Value);

            DateFrom = DateFrom != null ? new DateTime(DateFrom.Value.Year, DateFrom.Value.Month, DateFrom.Value.Day) : null;
            DateTo = DateTo != null ? new DateTime(DateTo.Value.Year, DateTo.Value.Month, DateTo.Value.Day) : null;

            if (DateFrom != null && DateTo != null)
                query = query.Where(n => n.Date >= DateFrom.Value && n.Date < DateTo.Value);

            else if (DateFrom != null && DateTo == null)
                query = query.Where(n => n.Date == DateFrom.Value);

            if (!string.IsNullOrEmpty(Notes))
                query = query.Where(n => n.Notes.Contains(Notes));

            query = query.Include(n => n.Student)
                         .Include(n => n.Course)
                         .Include(n => n.ClassRoom)
                         .Include(n => n.Supervisor);

            query = query.OrderByDescending(q => q.Date)
             .Skip((pageNumber - 1) * pagesize)
             .Take(pagesize);

            return await query.Select(n => new StudentNoteResultDTO
            {
                Id = n.Id,
                Date = n.Date,
                Notes = n.Notes,
                CourseId = n.CourseId,
                CourseName = n.Course != null ? n.Course.Name : string.Empty,
                SupervisorId = n.SupervisorId,
                SupervisorName = $"{n.Supervisor.FirstName} {n.Supervisor.MiddelName} {n.Supervisor.LastName}",
                ClassRoomId = n.ClassRoomId,
                ClassRoomName = n.ClassRoom.Name,
                StudentId = n.StudentId,
                StudentName = $"{n.Student.FirstName} {n.Student.MiddelName} {n.Student.LastName}"
            }).ToListAsync();

        }

        public async Task<StudentNoteResultDTO?> GetStudentNoteViewById(Guid StudentId)
        {
            var note = await context.studentNotes.Include(n => n.Student)
                                                 .Include(n => n.Course)
                                                 .Include(n => n.ClassRoom)
                                                 .Include(n => n.Supervisor)
                                                 .FirstOrDefaultAsync(n => n.Id == StudentId);

            return note == null ? null : new StudentNoteResultDTO
            {
                Id = note.Id,
                Date = note.Date,
                Notes = note.Notes,
                CourseId = note.CourseId,
                CourseName = note.Course != null ? note.Course.Name : string.Empty,
                SupervisorId = note.SupervisorId,
                SupervisorName = $"{note.Supervisor.FirstName} {note.Supervisor.MiddelName} {note.Supervisor.LastName}",
                ClassRoomId = note.ClassRoomId,
                ClassRoomName = note.ClassRoom.Name,
                StudentId = note.StudentId,
                StudentName = $"{note.Student.FirstName} {note.Student.MiddelName} {note.Student.LastName}"
            };

        }
            



    }
}
