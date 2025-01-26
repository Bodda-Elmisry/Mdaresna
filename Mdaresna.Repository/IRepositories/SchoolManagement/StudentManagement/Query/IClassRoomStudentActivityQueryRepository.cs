using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query
{
    public interface IClassRoomStudentActivityQueryRepository : IBaseQueryRepository<ClassRoomStudentActivity>
    {
        Task<IEnumerable<ClassRoomStudentActivityResultDTO>> GetStudentActivitiesListAsync(Guid? StudentId,
                                                                                                     Guid? ActivityId,
                                                                                                     decimal? ResultFrom,
                                                                                                     decimal? ResultTo,
                                                                                                     int pageNumber);

        Task<ClassRoomStudentActivity?> GetClassRoomStudentActivityAsync(Guid studentId, Guid ActivityId);

        Task<ClassRoomStudentActivityResultDTO?> GetClassRoomStudentActivityViewAsync(Guid studentId, Guid ActivityId);

    }
}
