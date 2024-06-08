using Mdaresna.Doamin.DTOs.ClassRoom;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query
{
    public interface IClassRoomQueryService : IBaseQueryService<ClassRoom>
    {
        Task<IEnumerable<ClassRoom>> GetBySchoolIdAsync(Guid SchoolId);
        Task<IEnumerable<ClassRoom>> GetBySchoolIdAndSupervisorIdAsync(Guid SchoolId, Guid SupervisorId);
        Task<ClassRoomHelpDataDTO> getInitialValue(Guid SchoolId);
    }
}
