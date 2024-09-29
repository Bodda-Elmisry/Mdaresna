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
    public class PermissionQueryService : BaseQueryService<Permission>, IPermissionQueryService
    {
        private readonly IBaseQueryRepository<Permission> queryRepository;
        private readonly IBaseSharedRepository<Permission> sharedRepository;
        private readonly IPermissionQueryRepository permissionQueryRepository;

        public PermissionQueryService(IBaseQueryRepository<Permission> queryRepository,
            IBaseSharedRepository<Permission> sharedRepository,
            IPermissionQueryRepository permissionQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
            this.permissionQueryRepository = permissionQueryRepository;
        }

        public async Task<IEnumerable<Permission>> GetPermissionsListAsync(int permissionsType, int pageNumber, string? permissionName)
        {
            return await permissionQueryRepository. GetPermissionsListAsync(permissionsType, pageNumber, permissionName);
        }
    }
}
