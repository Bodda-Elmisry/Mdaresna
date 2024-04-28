using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.UserManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.UserManagement.Query
{
    public class UserQueryRepository : BaseQueryRepository<User>, IUserQueryRepository
    {
        private readonly AppDbContext context;

        public UserQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<User?> GetUserByPhoneNumber(string PhoneNumber)
        {
            try
            {
                return await context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == PhoneNumber);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<User?> GetUserByPhoneNumberAndConfirmationKey(string PhoneNumber, string Key)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == PhoneNumber &&
                                                                u.PhoneConfirmationCode == Key);
        }

        public async Task<User?> GetUserByPhoneNumberAndPassword(string PhoneNumber, string Password)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == PhoneNumber &&
                                                                u.Password == Password);
        }
    }
}
