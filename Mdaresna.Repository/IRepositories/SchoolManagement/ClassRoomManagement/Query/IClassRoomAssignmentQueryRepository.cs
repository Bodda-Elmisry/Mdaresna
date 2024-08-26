using Mdaresna.Doamin.DTOs.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Query
{
    public interface IClassRoomAssignmentQueryRepository : IBaseQueryRepository<ClassRoomAssignment>
    {
        Task<IEnumerable<ClassRoomAssignmentResultDTO>> GetClassRoomAssignmentsList(Guid? classRoomId, Guid? SupervisorId, Guid? courseId, string? details, decimal? rate, DateTime? fromdate, DateTime? todate, int pageNumber);
        Task<ClassRoomAssignmentResultDTO?> GetClassRoomAssignmentById(Guid assignmentId);
    }
}
