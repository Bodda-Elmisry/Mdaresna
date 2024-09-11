using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Query
{
    public class StudentNoteQueryService : BaseQueryService<StudentNote>, IStudentNoteQueryService
    {
        private readonly IStudentNoteQueryRepository studentNoteQueryRepository;

        public StudentNoteQueryService(IBaseQueryRepository<StudentNote> queryRepository,
            IBaseSharedRepository<StudentNote> sharedRepository,
            IStudentNoteQueryRepository studentNoteQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.studentNoteQueryRepository = studentNoteQueryRepository;
        }

        public async Task<IEnumerable<StudentNoteResultDTO>> GetStudentNotesListAsync(Guid StudentId, Guid? ClassRoomId, Guid? SupervisorId, Guid? CourseId, DateTime? DateFrom, DateTime? DateTo, string? Notes)
        {
            return await studentNoteQueryRepository.GetStudentNotesListAsync(StudentId, ClassRoomId, SupervisorId, CourseId, DateFrom, DateTo, Notes);
        }

        public async Task<StudentNoteResultDTO?> GetStudentNoteViewById(Guid StudentId)
        {
            return await studentNoteQueryRepository.GetStudentNoteViewById(StudentId);
        }
    }
}
