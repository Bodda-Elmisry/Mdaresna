using Mdaresna.Doamin.DTOs.UserManagement;
using Mdaresna.Doamin.Helpers;
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
                return await context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == PhoneNumber && u.Deleted == false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<User?> GetUserByPhoneNumberAndConfirmationKey(string PhoneNumber, string Key)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == PhoneNumber &&
                                                                u.PhoneConfirmationCode == Key &&
                                                                u.Deleted == false);
        }

        public async Task<User?> GetUserByPhoneNumberAndPassword(string PhoneNumber, string Password)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == PhoneNumber &&
                                                                u.Password == Password &&
                                                                u.Deleted == false);
        }

        public async Task<UserResultDTO?> GetUserById(Guid Id)
        {
            var user =  await context.Users.FirstAsync(u => u.Id == Id && u.Deleted == false);

            return user != null ? new UserResultDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                MiddelName = user.MiddelName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                ImageUrl = !string.IsNullOrEmpty(user.ImageUrl) ? $"{SettingsHelper.GetAppUrl()}/{user.ImageUrl.Replace("\\", "/")}" : null,
                BirthDay = user.BirthDay,
                Address = user.Address,
                City = user.City,
                Region = user.Region,
                Country = user.Contry,
                Email = user.Email
            } : null;
        }
    }
}
