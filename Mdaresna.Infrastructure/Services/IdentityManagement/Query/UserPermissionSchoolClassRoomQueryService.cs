using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.IdentityManagement.Query
{
    public class UserPermissionSchoolClassRoomQueryService : BaseQueryService<UserPermissionSchoolClassRoom>, IUserPermissionSchoolClassRoomQueryService
    {
        private readonly IBaseQueryRepository<UserPermissionSchoolClassRoom> queryRepository;
        private readonly IBaseSharedRepository<UserPermissionSchoolClassRoom> sharedRepository;

        public UserPermissionSchoolClassRoomQueryService(IBaseQueryRepository<UserPermissionSchoolClassRoom> queryRepository,
            IBaseSharedRepository<UserPermissionSchoolClassRoom> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
