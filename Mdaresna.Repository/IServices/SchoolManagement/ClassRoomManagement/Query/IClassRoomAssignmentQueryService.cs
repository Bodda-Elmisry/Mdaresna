using Mdaresna.Doamin.DTOs.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query
{
    public interface IClassRoomAssignmentQueryService : IBaseQueryService<ClassRoomAssignment>
    {
        Task<IEnumerable<ClassRoomAssignmentResultDTO>> GetClassRoomAssignmentsList(Guid? classRoomId, Guid? SupervisorId, Guid? courseId, string? details, decimal? rate, DateTime? fromdate, DateTime? todate, int pageNumber);
        Task<ClassRoomAssignmentResultDTO?> GetClassRoomAssignmentById(Guid assignmentId);
    }
}
