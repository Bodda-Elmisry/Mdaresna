//using Mdaresna.Doamin.DTOs.StudentManagement;
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
    public interface IStudentParentQueryRepository : IBaseQueryRepository<StudentParent>
    {
        Task<IEnumerable<ParentStudentResultDTO>> GetParentStudentsAsync(Guid parentId, Guid? relationId);
        Task<IEnumerable<StudentParentResultDTO>> GetstudentParentsAsync(Guid studentId, Guid? relationId);
        Task<StudentParentResultDTO?> GetstudentParentAsync(Guid parentId, Guid studentId);
        Task<StudentParent?> GetstudentParentByIdAsync(Guid parentId, Guid studentId);
    }
}
