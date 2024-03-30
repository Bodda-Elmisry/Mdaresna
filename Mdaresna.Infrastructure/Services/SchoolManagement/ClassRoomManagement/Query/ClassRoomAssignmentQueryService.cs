using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.ClassRoomManagement.Query
{
    public class ClassRoomAssignmentQueryService : BaseQueryService<ClassRoomAssignment>, IClassRoomAssignmentQueryService
    {
        private readonly IBaseQueryRepository<ClassRoomAssignment> queryRepository;
        private readonly IBaseSharedRepository<ClassRoomAssignment> sharedRepository;

        public ClassRoomAssignmentQueryService(IBaseQueryRepository<ClassRoomAssignment> queryRepository,
            IBaseSharedRepository<ClassRoomAssignment> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
