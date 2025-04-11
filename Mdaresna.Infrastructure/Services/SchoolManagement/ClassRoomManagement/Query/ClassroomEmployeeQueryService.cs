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
    public class ClassroomEmployeeQueryService : BaseQueryService<ClassroomEmployee>, IClassroomEmployeeQueryService
    {
        private readonly IBaseQueryRepository<ClassroomEmployee> queryRepository;
        private readonly IBaseSharedRepository<ClassroomEmployee> sharedRepository;
        private readonly IClassroomEmployeeQueryRepository classroomEmployeeQueryRepository;

        public ClassroomEmployeeQueryService(IBaseQueryRepository<ClassroomEmployee> queryRepository, 
                                             IBaseSharedRepository<ClassroomEmployee> sharedRepository,
                                             IClassroomEmployeeQueryRepository classroomEmployeeQueryRepository)
            : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
            this.classroomEmployeeQueryRepository = classroomEmployeeQueryRepository;
        }

        public async Task<ClassroomEmployee?> GetByIdAsync(Guid employeeId, Guid roomId)
        {
            return await classroomEmployeeQueryRepository.GetByIdAsync(employeeId, roomId);
        }

        public async Task<ClassroomEmployeeResultDTO?> GetClassroomEmployeeAsync(Guid employeeId, Guid? roomId)
        {
            return await classroomEmployeeQueryRepository.GetClassroomEmployeeAsync(employeeId, roomId);
        }

        public async Task<IEnumerable<ClassroomEmployeeResultDTO>?> GetClassroomsEmployeesAsync(Guid? employeeId, Guid? roomId, Guid schoolId)
        {
            return await classroomEmployeeQueryRepository.GetClassroomsEmployeesAsync(employeeId, roomId, schoolId);
        }

        public async Task<IEnumerable<ClassroomEmployeeResultDTO>> GetEmployeeDataAsync(Guid employeeId)
        {
            return await classroomEmployeeQueryRepository.GetEmployeeDataAsync(employeeId);
        }

        public async Task<bool> IsExistAsync(Guid employeeId, Guid roomId)
        {
            return await classroomEmployeeQueryRepository.IsExistAsync(employeeId, roomId);
        }
    }
}
