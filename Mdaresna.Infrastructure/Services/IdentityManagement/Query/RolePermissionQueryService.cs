using Mdaresna.Doamin.DTOs.Identity;
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
    public class RolePermissionQueryService : BaseQueryService<RolePermission>, IRolePermissionQueryService
    {
        private readonly IRolePermissionQueryRepository rolePermissionQueryRepository;

        public RolePermissionQueryService(IBaseQueryRepository<RolePermission> queryRepository,
            IBaseSharedRepository<RolePermission> sharedRepository,
            IRolePermissionQueryRepository rolePermissionQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.rolePermissionQueryRepository = rolePermissionQueryRepository;
        }

        public async Task<IEnumerable<RolePermissionResultDTO>> GetRolePermissions(Guid roleId)
        {
            return await rolePermissionQueryRepository.GetRolePermissions(roleId);
        }
    }
}
