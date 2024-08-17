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
    public class ClassRoomExamQueryService : BaseQueryService<ClassRoomExam>, IClassRoomExamQueryService
    {
        private readonly IClassRoomExamQueryRepository classRoomExamQueryRepository;

        public ClassRoomExamQueryService(IBaseQueryRepository<ClassRoomExam> queryRepository,
            IBaseSharedRepository<ClassRoomExam> sharedRepository,
            IClassRoomExamQueryRepository classRoomExamQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.classRoomExamQueryRepository = classRoomExamQueryRepository;
        }

        public async Task<IEnumerable<ClassRoomExamResultDTO>> GetExamsList(IEnumerable<Guid> months, DateTime? fromDate, DateTime? toDate, 
                                                                      string weekDay, Guid? classRoomId, Guid? supervisorId, Guid? courseId, decimal? rate)
        {
            return await classRoomExamQueryRepository.GetExamsList(months, fromDate, toDate, weekDay, classRoomId, supervisorId, courseId, rate);
        }

        public async Task<CreateClassRoomExamInitialDataDTO> GetInitialData(Guid schoolId)
        {
            return await classRoomExamQueryRepository.GetInitialData(schoolId);
        }

        public async Task<ClassRoomExamResultDTO?> GetExamByIdAsync(Guid examid)
        {
            return await classRoomExamQueryRepository.GetExamByIdAsync(examid);
        }
    }
}
