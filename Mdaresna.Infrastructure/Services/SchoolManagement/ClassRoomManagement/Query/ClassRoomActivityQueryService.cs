using Mdaresna.Doamin.DTOs.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
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
    public class ClassRoomActivityQueryService : BaseQueryService<ClassRoomActivity>, IClassRoomActivityQueryService
    {
        private readonly IBaseQueryRepository<ClassRoomActivity> queryRepository;
        private readonly IBaseSharedRepository<ClassRoomActivity> sharedRepository;
        private readonly IClassRoomActivityQueryRepository classRoomActivityQueryRepository;

        public ClassRoomActivityQueryService(IBaseQueryRepository<ClassRoomActivity> queryRepository,
            IBaseSharedRepository<ClassRoomActivity> sharedRepository,
            IClassRoomActivityQueryRepository classRoomActivityQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
            this.classRoomActivityQueryRepository = classRoomActivityQueryRepository;
        }

        public async Task<IEnumerable<ClassRoomActivityResultDTO>> GetClassRoomActivitiesList(Guid? classRoomId, Guid? SupervisorId, Guid? courseId, string? details, decimal? rate, DateTime? fromdate, DateTime? todate, int pageNumber)
        {
            return await classRoomActivityQueryRepository.GetClassRoomActivitiesList(classRoomId,
                                                                                     SupervisorId, 
                                                                                     courseId, 
                                                                                     details, 
                                                                                     rate,
                                                                                     fromdate,
                                                                                     todate,
                                                                                     pageNumber);
        }

        public async Task<ClassRoomActivityResultDTO?> GetClassRoomActivityById(Guid activityId)
        {
            return await classRoomActivityQueryRepository.GetClassRoomActivityById(activityId);
        }
    }
}
