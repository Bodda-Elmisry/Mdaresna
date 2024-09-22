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
    public class UserRoleQueryService : BaseQueryService<UserRole>, IUserRoleQueryService
    {
        private readonly IBaseQueryRepository<UserRole> queryRepository;
        private readonly IBaseSharedRepository<UserRole> sharedRepository;
        private readonly IUserRoleQueryRepository userRoleQueryRepository;

        public UserRoleQueryService(IBaseQueryRepository<UserRole> queryRepository,
            IBaseSharedRepository<UserRole> sharedRepository,
            IUserRoleQueryRepository userRoleQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
            this.userRoleQueryRepository = userRoleQueryRepository;
        }

        public async Task<bool> CheckRoleExist(UserRole role)
        {
            return await userRoleQueryRepository.CheckUserRole(role);
        }

        public async Task<UserRoleResultDTO?> GetUserRoleAsync(Guid userId, Guid roleId)
        {
            return await userRoleQueryRepository.GetUserRoleAsync(userId, roleId);
        }

        public async Task<IEnumerable<UserRoleResultDTO>> GetUserRolesAsync(Guid userId, Guid? schoolId)
        {
            return await userRoleQueryRepository.GetUserRolesAsync(userId, schoolId);
        }
    }
}
