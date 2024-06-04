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
        Task<IEnumerable<ClassRoom>> GetBySchoolIdAsync(Guid SchoolId);
        Task<IEnumerable<ClassRoom>> GetBySchoolIdAndSupervisorIdAsync(Guid SchoolId, Guid SupervisorId);

    }
}
