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
    public class UserRoleQueryService : BaseQueryService<UserRole>, IUserRoleQueryService
    {
        private readonly IBaseQueryRepository<UserRole> queryRepository;
        private readonly IBaseSharedRepository<UserRole> sharedRepository;

        public UserRoleQueryService(IBaseQueryRepository<UserRole> queryRepository,
            IBaseSharedRepository<UserRole> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
