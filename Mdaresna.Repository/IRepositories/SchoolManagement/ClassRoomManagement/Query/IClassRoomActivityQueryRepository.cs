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
    public interface IClassRoomActivityQueryRepository : IBaseQueryRepository<ClassRoomActivity>
    {
        Task<IEnumerable<ClassRoomActivityResultDTO>> GetClassRoomActivitiesList(Guid? classRoomId, Guid? SupervisorId, Guid? courseId, string? details, decimal? rate, DateTime? fromdate, DateTime? todate, int pageNumber);

        Task<ClassRoomActivityResultDTO?> GetClassRoomActivityById(Guid activityId);
    }
}
