using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.UserManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.UserManagement.Query
{
    public class UserQueryService : BaseQueryService<User>, IUserQueryService
    {
        private readonly IBaseQueryRepository<User> queryRepository;
        private readonly IBaseSharedRepository<User> sharedRepository;

        public UserQueryService(IBaseQueryRepository<User> queryRepository,
            IBaseSharedRepository<User> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
