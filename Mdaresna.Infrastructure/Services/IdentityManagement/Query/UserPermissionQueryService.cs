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
    public class UserPermissionQueryService : BaseQueryService<UserPermission>, IUserPermissionQueryService
    {
        private readonly IBaseQueryRepository<UserPermission> queryRepository;
        private readonly IBaseSharedRepository<UserPermission> sharedRepository;

        public UserPermissionQueryService(IBaseQueryRepository<UserPermission> queryRepository,
            IBaseSharedRepository<UserPermission> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
