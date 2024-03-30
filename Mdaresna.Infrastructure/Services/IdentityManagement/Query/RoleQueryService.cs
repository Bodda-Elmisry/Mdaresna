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
    public class RoleQueryService : BaseQueryService<Role>, IRoleQueryService
    {
        private readonly IBaseQueryRepository<Role> queryRepository;
        private readonly IBaseSharedRepository<Role> sharedRepository;

        public RoleQueryService(IBaseQueryRepository<Role> queryRepository,
            IBaseSharedRepository<Role> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
