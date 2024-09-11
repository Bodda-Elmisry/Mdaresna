using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query
{
    public interface IStudentNoteQueryService : IBaseQueryService<StudentNote>
    {
        Task<IEnumerable<StudentNoteResultDTO>> GetStudentNotesListAsync(Guid StudentId, Guid? ClassRoomId, Guid? SupervisorId, Guid? CourseId, DateTime? DateFrom, DateTime? DateTo, string? Notes);

        Task<StudentNoteResultDTO?> GetStudentNoteViewById(Guid StudentId);
    }
}
