using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.UserManagement.Query
{
    public interface IUserQueryService : IBaseQueryService<User>
    {
        Task<User> GetUserByPhoneNumber(string PhoneNumber);

        Task<User> GetUserByPhoneNumberAndConfirmationKey(string PhoneNumber, string Key);
    }
}
