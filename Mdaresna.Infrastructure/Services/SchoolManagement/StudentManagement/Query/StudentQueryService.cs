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
    public class StudentQueryService : BaseQueryService<Student>, IStudentQueryService
    {
        private readonly IBaseQueryRepository<Student> queryRepository;
        private readonly IBaseSharedRepository<Student> sharedRepository;
        private readonly IStudentQueryRepository studentQueryRepository;

        public StudentQueryService(IBaseQueryRepository<Student> queryRepository,
            IBaseSharedRepository<Student> sharedRepository,
            IStudentQueryRepository studentQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
            this.studentQueryRepository = studentQueryRepository;
        }

        public async Task<IEnumerable<StudentResultDTO>> GetStudentsBySchoolIdAsync(Guid schoolId, string studentCode, string studentName)
        {
            return await studentQueryRepository.GetStudentsBySchoolIdAsync(schoolId, studentCode, studentName);
        }

        public async Task<IEnumerable<StudentResultDTO>> GetStudentsBySchoolIdAndClassRoomIdAsync(Guid schoolId, Guid classroomId)
        {
            return await studentQueryRepository.GetStudentsBySchoolIdAndClassRoomIdAsync(schoolId, classroomId);
        }

        public async Task<StudentResultDTO?> GetStudentByIdAsync(Guid studentId)
        {
            return await studentQueryRepository.GetStudentByIdAsync(studentId);
        }

        public async Task<StudentResultDTO?> GetStudentByCodeAsync(string code)
        {
            return await studentQueryRepository.GetStudentByCodeAsync(code);
        }

        public async Task<string> GetMaxStudebtCodeAsync(Guid schoolId)
        {
            return await studentQueryRepository.GetMaxStudebtCodeAsync(schoolId);
        }
    }
}
