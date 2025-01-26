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
    public class ClassRoomStudentAssignmentQueryService : BaseQueryService<ClassRoomStudentAssignment>, IClassRoomStudentAssignmentQueryService
    {
        private readonly IClassRoomStudentAssignmentQueryRepository classRoomStudentAssignmentQueryRepository;

        public ClassRoomStudentAssignmentQueryService(IBaseQueryRepository<ClassRoomStudentAssignment> queryRepository,
            IBaseSharedRepository<ClassRoomStudentAssignment> sharedRepository,
            IClassRoomStudentAssignmentQueryRepository classRoomStudentAssignmentQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.classRoomStudentAssignmentQueryRepository = classRoomStudentAssignmentQueryRepository;
        }

        public async Task<ClassRoomStudentAssignment?> GetClassRoomStudentAssignmentAsync(Guid studentId, Guid AssignmentId)
        {
            return await classRoomStudentAssignmentQueryRepository.GetClassRoomStudentAssignmentAsync(studentId, AssignmentId);
        }

        public async Task<ClassRoomStudentAssignmentResultDTO?> GetClassRoomStudentAssignmentViewAsync(Guid studentId, Guid AssignmentId)
        {
            return await classRoomStudentAssignmentQueryRepository.GetClassRoomStudentAssignmentViewAsync(studentId, AssignmentId);
        }

        public async Task<IEnumerable<ClassRoomStudentAssignmentResultDTO>> GetStudentAssignmentsListAsync(Guid? StudentId, Guid? AssignementId, decimal? ResultFrom, decimal? ResultTo, bool? IsDelivered, DateTime? DeliveredDateFrom, DateTime? DeliveredDateTo, int pageNumber)
        {
            return await classRoomStudentAssignmentQueryRepository.GetStudentAssignmentsListAsync(StudentId, AssignementId, ResultFrom, ResultTo, IsDelivered, DeliveredDateFrom, DeliveredDateTo, pageNumber);
        }
    }
}
