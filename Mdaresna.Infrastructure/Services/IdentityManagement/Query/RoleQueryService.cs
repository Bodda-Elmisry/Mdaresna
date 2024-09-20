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
    public class RoleQueryService : BaseQueryService<Role>, IRoleQueryService
    {
        private readonly IBaseQueryRepository<Role> queryRepository;
        private readonly IBaseSharedRepository<Role> sharedRepository;
        private readonly IRoleQueryRepository roleQueryRepository;

        public RoleQueryService(IBaseQueryRepository<Role> queryRepository,
            IBaseSharedRepository<Role> sharedRepository,
            IRoleQueryRepository roleQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
            this.roleQueryRepository = roleQueryRepository;
        }

        public async Task<IEnumerable<RoleResultDTO>> GetRolesAsync(int type, string? name, bool? activation, string? description)
        {
            return await roleQueryRepository.GetRolesAsync(type, name, activation, description);
        }

        public async Task<Role?> GetStanderdRole()
        {
            return await roleQueryRepository.GetStanderdRole();
        }
    }
}
