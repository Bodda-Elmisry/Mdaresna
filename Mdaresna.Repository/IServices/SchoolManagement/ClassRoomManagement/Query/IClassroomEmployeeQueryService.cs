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
    public interface IClassroomEmployeeQueryService : IBaseQueryService<ClassroomEmployee>
    {
        Task<IEnumerable<ClassroomEmployeeResultDTO>> GetEmployeeDataAsync(Guid employeeId);
        Task<IEnumerable<ClassroomEmployeeResultDTO>?> GetClassroomsEmployeesAsync(Guid? employeeId, Guid? roomId, Guid schoolId);
        Task<ClassroomEmployeeResultDTO?> GetClassroomEmployeeAsync(Guid employeeId, Guid? roomId);
        Task<ClassroomEmployee?> GetByIdAsync(Guid employeeId, Guid roomId);
        Task<bool> IsExistAsync(Guid employeeId, Guid roomId);
    }
}
