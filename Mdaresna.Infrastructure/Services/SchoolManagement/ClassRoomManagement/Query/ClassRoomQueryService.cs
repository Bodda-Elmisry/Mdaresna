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
    public class ClassRoomQueryService : BaseQueryService<ClassRoom>, IClassRoomQueryService
    {
        private readonly IBaseQueryRepository<ClassRoom> queryRepository;
        private readonly IBaseSharedRepository<ClassRoom> sharedRepository;

        public ClassRoomQueryService(IBaseQueryRepository<ClassRoom> queryRepository,
            IBaseSharedRepository<ClassRoom> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
