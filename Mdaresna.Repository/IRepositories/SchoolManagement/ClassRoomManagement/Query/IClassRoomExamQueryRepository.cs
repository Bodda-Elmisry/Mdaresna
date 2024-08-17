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
    public interface IClassRoomExamQueryRepository : IBaseQueryRepository<ClassRoomExam>
    {
        Task<IEnumerable<ClassRoomExamResultDTO>> GetExamsList(IEnumerable<Guid> months, DateTime? fromDate, DateTime? toDate,
                                                                      string weekDay, Guid? classRoomId, Guid? supervisorId,
                                                                      Guid? courseId, decimal? rate);

        Task<CreateClassRoomExamInitialDataDTO> GetInitialData(Guid schoolId);
        Task<ClassRoomExamResultDTO?> GetExamByIdAsync(Guid examid);
    }
}
