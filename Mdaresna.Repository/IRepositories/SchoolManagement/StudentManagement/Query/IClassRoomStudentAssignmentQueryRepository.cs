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
    public interface IClassRoomStudentAssignmentQueryRepository : IBaseQueryRepository<ClassRoomStudentAssignment>
    {
        Task<IEnumerable<ClassRoomStudentAssignmentResultDTO>> GetStudentAssignmentsListAsync(Guid StudentId,
                                                                                                     Guid? AssignementId,
                                                                                                     decimal? ResultFrom,
                                                                                                     decimal? ResultTo,
                                                                                                     bool? IsDelivered,
                                                                                                     DateTime? DeliveredDateFrom,
                                                                                                     DateTime? DeliveredDateTo,
                                                                                                     int pageNumber);
        Task<ClassRoomStudentAssignment?> GetClassRoomStudentAssignmentAsync(Guid studentId, Guid AssignmentId);
        Task<ClassRoomStudentAssignmentResultDTO?> GetClassRoomStudentAssignmentViewAsync(Guid studentId, Guid AssignmentId);
    }
}
