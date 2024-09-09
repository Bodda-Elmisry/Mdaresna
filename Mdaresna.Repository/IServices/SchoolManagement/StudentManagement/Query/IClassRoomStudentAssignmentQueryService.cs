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
    public interface IClassRoomStudentAssignmentQueryService : IBaseQueryService<ClassRoomStudentAssignment>
    {
        Task<IEnumerable<ClassRoomStudentAssignmentResultDTO>> GetStudentAssignmentsListAsync(Guid StudentId,
                                                                                                     Guid? AssignementId,
                                                                                                     decimal? ResultFrom,
                                                                                                     decimal? ResultTo,
                                                                                                     bool? IsDelivered,
                                                                                                     DateTime? DeliveredDateFrom,
                                                                                                     DateTime? DeliveredDateTo);
        Task<ClassRoomStudentAssignment?> GetClassRoomStudentAssignmentAsync(Guid studentId, Guid AssignmentId);
        Task<ClassRoomStudentAssignmentResultDTO?> GetClassRoomStudentAssignmentViewAsync(Guid studentId, Guid AssignmentId);
    }
}
