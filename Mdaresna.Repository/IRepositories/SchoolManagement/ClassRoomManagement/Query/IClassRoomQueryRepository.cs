using Mdaresna.Doamin.DTOs.ClassRoom;
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
    public interface IClassRoomQueryRepository : IBaseQueryRepository<ClassRoom>
    {
        Task<IEnumerable<ClassRoomResultDTO>> GetBySchoolIdAsync(Guid SchoolId);
        Task<IEnumerable<ClassRoomResultDTO>> GetBySchoolIdAndSupervisorIdAsync(Guid SchoolId, Guid SupervisorId);
        Task<IEnumerable<ClassRoomResultDTO>> GetBySchoolIdAndUserIdAsync(Guid schoolId, Guid userId);
        Task<ClassRoomResultDTO?> GetClassRoomByIdAsync(Guid roomId);

    }
}
