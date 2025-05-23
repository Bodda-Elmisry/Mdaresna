using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query
{
    public interface IClassRoomStudentExamQueryRepository : IBaseQueryRepository<ClassRoomStudentExam>
    {
        Task<IEnumerable<ClassRoomStudentExamResultDTO>> GetClassRoomStudentExamsListAsync(Guid? StudentId, Guid? ExamId, decimal? TotalResultFrom, decimal? TotalResultTo, bool? IsAttend, int pageNumber);

        Task<IEnumerable<ClassRoomStudentExam>> GetClassRoomStudentExamsListAsync(Guid ExamId);

        Task<ClassRoomStudentExamResultDTO?> GetClassRoomStudentExamViewAsync(Guid studentId, Guid ExamId);

        Task<ClassRoomStudentExam?> GetClassRoomStudentExamAsync(Guid studentId, Guid ExamId);
    }
}
