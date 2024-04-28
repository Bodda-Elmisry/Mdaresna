using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.UserManagement.Query
{
    public interface IUserQueryRepository : IBaseQueryRepository<User>
    {
        Task<User?> GetUserByPhoneNumber(string PhoneNumber);
        Task<User?> GetUserByPhoneNumberAndConfirmationKey(string PhoneNumber, string Key);

        Task<User?> GetUserByPhoneNumberAndPassword(string PhoneNumber, string Password);
    }
}
