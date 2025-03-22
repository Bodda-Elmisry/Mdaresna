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
    public class ClassRoomStudentExamQueryService : BaseQueryService<ClassRoomStudentExam>, IClassRoomStudentExamQueryService
    {
        private readonly IClassRoomStudentExamQueryRepository classRoomStudentExamQueryRepository;

        public ClassRoomStudentExamQueryService(IBaseQueryRepository<ClassRoomStudentExam> queryRepository,
            IBaseSharedRepository<ClassRoomStudentExam> sharedRepository,
            IClassRoomStudentExamQueryRepository classRoomStudentExamQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.classRoomStudentExamQueryRepository = classRoomStudentExamQueryRepository;
        }

        public async Task<ClassRoomStudentExam?> GetClassRoomStudentExamAsync(Guid studentId, Guid ExamId)
        {
            return await classRoomStudentExamQueryRepository.GetClassRoomStudentExamAsync(studentId, ExamId);
        }

        public async Task<IEnumerable<ClassRoomStudentExamResultDTO>> GetClassRoomStudentExamsListAsync(Guid? StudentId, Guid? ExamId, decimal? TotalResultFrom, decimal? TotalResultTo, bool? IsAttend, int pageNumber)
        {
            return await classRoomStudentExamQueryRepository.GetClassRoomStudentExamsListAsync(StudentId, ExamId, TotalResultFrom, TotalResultTo, IsAttend, pageNumber);
        }

        public async Task<ClassRoomStudentExamResultDTO> GetClassRoomStudentExamViewAsync(Guid studentId, Guid ExamId)
        {
            return await classRoomStudentExamQueryRepository.GetClassRoomStudentExamViewAsync(studentId, ExamId);
        }
    }
}
