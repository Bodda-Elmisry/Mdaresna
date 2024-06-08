using Mdaresna.Doamin.DTOs.ClassRoom;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.ClassRoomManagement.Query
{
    public class ClassRoomQueryService : BaseQueryService<ClassRoom>, IClassRoomQueryService
    {
        private readonly IBaseQueryRepository<ClassRoom> queryRepository;
        private readonly IBaseSharedRepository<ClassRoom> sharedRepository;
        private readonly IClassRoomQueryRepository classRoomQueryRepository;
        private readonly IClassRoomHelpDataQueryRepository classRoomHelpDataQueryRepository;

        public ClassRoomQueryService(IBaseQueryRepository<ClassRoom> queryRepository,
            IBaseSharedRepository<ClassRoom> sharedRepository,
            IClassRoomQueryRepository classRoomQueryRepository,
            IClassRoomHelpDataQueryRepository classRoomHelpDataQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
            this.classRoomQueryRepository = classRoomQueryRepository;
            this.classRoomHelpDataQueryRepository = classRoomHelpDataQueryRepository;
        }

        public async Task<IEnumerable<ClassRoom>> GetBySchoolIdAsync(Guid SchoolId)
        {
            return await classRoomQueryRepository.GetBySchoolIdAsync(SchoolId);
        }

        public async Task<IEnumerable<ClassRoom>> GetBySchoolIdAndSupervisorIdAsync(Guid SchoolId, Guid SupervisorId)
        {
            return await classRoomQueryRepository.GetBySchoolIdAndSupervisorIdAsync(SchoolId, SupervisorId);
        }

        public async Task<ClassRoomHelpDataDTO> getInitialValue(Guid SchoolId)
        {
            return await classRoomHelpDataQueryRepository.GetClassRoomHrlpDataAsync(SchoolId);
        }
    }
}
