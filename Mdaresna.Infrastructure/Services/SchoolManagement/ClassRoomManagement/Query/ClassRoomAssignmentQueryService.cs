using Mdaresna.Doamin.DTOs.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.ClassRoomManagement.Query
{
    public class ClassRoomAssignmentQueryService : BaseQueryService<ClassRoomAssignment>, IClassRoomAssignmentQueryService
    {
        private readonly IClassRoomAssignmentQueryRepository classRoomAssignmentQueryRepository;

        public ClassRoomAssignmentQueryService(IBaseQueryRepository<ClassRoomAssignment> queryRepository,
            IBaseSharedRepository<ClassRoomAssignment> sharedRepository,
            IClassRoomAssignmentQueryRepository classRoomAssignmentQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.classRoomAssignmentQueryRepository = classRoomAssignmentQueryRepository;
        }

        public async Task<ClassRoomAssignmentResultDTO?> GetClassRoomAssignmentById(Guid assignmentId)
        {
            return await classRoomAssignmentQueryRepository.GetClassRoomAssignmentById(assignmentId);
        }

        public async Task<IEnumerable<ClassRoomAssignmentResultDTO>> GetClassRoomAssignmentsList(Guid? classRoomId, Guid? SupervisorId, Guid? courseId, string? details, decimal? rate, DateTime? fromdate, DateTime? todate, int pageNumber)
        {
            return await classRoomAssignmentQueryRepository.GetClassRoomAssignmentsList(classRoomId,
                                                                                        SupervisorId, 
                                                                                        courseId, 
                                                                                        details, 
                                                                                        rate, 
                                                                                        fromdate, 
                                                                                        todate,
                                                                                        pageNumber);
        }




    }
}
