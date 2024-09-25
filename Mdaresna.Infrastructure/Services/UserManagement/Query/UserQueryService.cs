using Mdaresna.Doamin.DTOs.UserManagement;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.UserManagement.Query;
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
        private readonly IUserQueryRepository userQueryRepository;

        public UserQueryService(IBaseQueryRepository<User> queryRepository,
            IBaseSharedRepository<User> sharedRepository,
            IUserQueryRepository userQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
            this.userQueryRepository = userQueryRepository;
        }

        public async Task<UserResultDTO> GetUserById(Guid Id)
        {
            return await userQueryRepository.GetUserById(Id);
        }

        public async Task<User> GetUserByPhoneNumber(string PhoneNumber)
        {
            return await userQueryRepository.GetUserByPhoneNumber(PhoneNumber);
        }

        public async Task<User> GetUserByPhoneNumberAndConfirmationKey(string PhoneNumber, string Key)
        {
            return await userQueryRepository.GetUserByPhoneNumberAndConfirmationKey(PhoneNumber, Key);
        }

        public async Task<User> GetUserByPhoneNumberAndPassword(string PhoneNumber, string Password)
        {
            return await userQueryRepository.GetUserByPhoneNumberAndPassword(PhoneNumber, Password);
        }
    }
}
