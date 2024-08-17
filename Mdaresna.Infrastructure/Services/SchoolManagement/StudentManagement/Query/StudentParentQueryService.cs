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
    public class StudentParentQueryService : BaseQueryService<StudentParent>, IStudentParentQueryService
    {
        private readonly IStudentParentQueryRepository studentParentQueryRepository;

        public StudentParentQueryService(IBaseQueryRepository<StudentParent> queryRepository,
            IBaseSharedRepository<StudentParent> sharedRepository,
            IStudentParentQueryRepository studentParentQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.studentParentQueryRepository = studentParentQueryRepository;
        }

        public async Task<IEnumerable<StudentParentResultDTO>> GetParentStudentsAsync(Guid parentId, Guid? relationId)
        {
            return await studentParentQueryRepository.GetParentStudentsAsync(parentId, relationId);
        }

        public async Task<StudentParentResultDTO?> GetstudentParentAsync(Guid parentId, Guid studentId)
        {
            return await studentParentQueryRepository.GetstudentParentAsync(parentId, studentId);
        }

        public async Task<StudentParent?> GetstudentParentByIdAsync(Guid parentId, Guid studentId)
        {
            return await studentParentQueryRepository.GetstudentParentByIdAsync(parentId, studentId);
        }

        public async Task<IEnumerable<StudentParentResultDTO>> GetstudentParentsAsync(Guid studentId, Guid? relationId)
        {
            return await studentParentQueryRepository.GetstudentParentsAsync(studentId, relationId);
        }
    }
}
