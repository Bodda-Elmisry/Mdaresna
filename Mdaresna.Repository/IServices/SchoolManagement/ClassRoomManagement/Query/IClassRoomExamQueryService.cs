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
    public interface IClassRoomExamQueryService : IBaseQueryService<ClassRoomExam>
    {
        Task<IEnumerable<ClassRoomExamResultDTO>> GetExamsList(IEnumerable<Guid> months, DateTime? fromDate, DateTime? toDate,
                                                                      string weekDay, Guid? classRoomId, Guid? supervisorId,
                                                                      Guid? courseId, decimal? rate);

        Task<CreateClassRoomExamInitialDataDTO> GetInitialData(Guid schoolId);
    }
}
