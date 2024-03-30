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
    public class ClassRoomActivityQueryService : BaseQueryService<ClassRoomActivity>, IClassRoomActivityQueryService
    {
        private readonly IBaseQueryRepository<ClassRoomActivity> queryRepository;
        private readonly IBaseSharedRepository<ClassRoomActivity> sharedRepository;

        public ClassRoomActivityQueryService(IBaseQueryRepository<ClassRoomActivity> queryRepository,
            IBaseSharedRepository<ClassRoomActivity> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
