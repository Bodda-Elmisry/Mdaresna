using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.IdentityManagement.Query;
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
        private readonly IUserPermissionSchoolClassRoomQueryRepository userPermissionSchoolClassRoomQueryRepository;

        public UserPermissionSchoolClassRoomQueryService(IBaseQueryRepository<UserPermissionSchoolClassRoom> queryRepository,
            IBaseSharedRepository<UserPermissionSchoolClassRoom> sharedRepository,
            IUserPermissionSchoolClassRoomQueryRepository userPermissionSchoolClassRoomQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
            this.userPermissionSchoolClassRoomQueryRepository = userPermissionSchoolClassRoomQueryRepository;
        }

        public async Task<UserPermissionSchoolClassRoom?> GetUserPermissionSchoolClassRoomByIdAsync(Guid userId, Guid permissionId, Guid classroomId)
        {
            return await userPermissionSchoolClassRoomQueryRepository.GetUserPermissionSchoolClassRoomByIdAsync(userId, permissionId, classroomId);
        }









    }
}
