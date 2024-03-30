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
    public class RolePermissionQueryService : BaseQueryService<RolePermission>, IRolePermissionQueryService
    {
        private readonly IBaseQueryRepository<RolePermission> queryRepository;
        private readonly IBaseSharedRepository<RolePermission> sharedRepository;

        public RolePermissionQueryService(IBaseQueryRepository<RolePermission> queryRepository,
            IBaseSharedRepository<RolePermission> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
