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
    public interface IClassRoomStudentExamQueryService : IBaseQueryService<ClassRoomStudentExam>
    {
        Task<IEnumerable<ClassRoomStudentExamResultDTO>> GetClassRoomStudentExamsListAsync(Guid? StudentId, Guid? ExamId, decimal? TotalResultFrom, decimal? TotalResultTo, bool? IsAttend, int pageNumber);

        Task<ClassRoomStudentExamResultDTO> GetClassRoomStudentExamViewAsync(Guid studentId, Guid ExamId);

        Task<ClassRoomStudentExam?> GetClassRoomStudentExamAsync(Guid studentId, Guid ExamId);
    }
}
