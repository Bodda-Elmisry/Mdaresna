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
    public interface IClassroomEmployeeQueryRepository : IBaseQueryRepository<ClassroomEmployee>
    {
        Task<IEnumerable<ClassroomEmployeeResultDTO>> GetEmployeeDataAsync(Guid employeeId);
        Task<IEnumerable<ClassroomEmployeeResultDTO>?> GetClassroomsEmployeesAsync(Guid? employeeId, Guid? roomId, Guid schoolId);
        Task<ClassroomEmployeeResultDTO?> GetClassroomEmployeeAsync(Guid employeeId, Guid? roomId);
        Task<ClassroomEmployee?> GetByIdAsync(Guid employeeId, Guid roomId);
        Task<bool> IsExistAsync(Guid employeeId, Guid roomId);
    }
}
