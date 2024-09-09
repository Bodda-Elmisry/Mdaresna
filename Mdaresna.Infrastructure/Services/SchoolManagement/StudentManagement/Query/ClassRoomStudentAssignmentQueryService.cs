using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Query
{
    public class ClassRoomStudentAssignmentQueryService : BaseQueryService<ClassRoomStudentAssignment>, IClassRoomStudentAssignmentQueryService
    {
        private readonly IClassRoomStudentAssignmentQueryService classRoomStudentAssignmentQueryService;

        public ClassRoomStudentAssignmentQueryService(IBaseQueryRepository<ClassRoomStudentAssignment> queryRepository,
            IBaseSharedRepository<ClassRoomStudentAssignment> sharedRepository,
            IClassRoomStudentAssignmentQueryService classRoomStudentAssignmentQueryService) 
                : base(queryRepository, sharedRepository)
        {
            this.classRoomStudentAssignmentQueryService = classRoomStudentAssignmentQueryService;
        }

        public async Task<ClassRoomStudentAssignment?> GetClassRoomStudentAssignmentAsync(Guid studentId, Guid AssignmentId)
        {
            return await classRoomStudentAssignmentQueryService.GetClassRoomStudentAssignmentAsync(studentId, AssignmentId);
        }

        public async Task<ClassRoomStudentAssignmentResultDTO?> GetClassRoomStudentAssignmentViewAsync(Guid studentId, Guid AssignmentId)
        {
            return await classRoomStudentAssignmentQueryService.GetClassRoomStudentAssignmentViewAsync(studentId, AssignmentId);
        }

        public async Task<IEnumerable<ClassRoomStudentAssignmentResultDTO>> GetStudentAssignmentsListAsync(Guid StudentId, Guid? AssignementId, decimal? ResultFrom, decimal? ResultTo, bool? IsDelivered, DateTime? DeliveredDateFrom, DateTime? DeliveredDateTo)
        {
            return await GetStudentAssignmentsListAsync(StudentId, AssignementId, ResultFrom, ResultTo, IsDelivered, DeliveredDateFrom, DeliveredDateTo);
        }
    }
}
