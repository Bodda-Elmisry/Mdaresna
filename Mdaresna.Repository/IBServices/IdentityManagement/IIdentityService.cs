using Mdaresna.Doamin.DTOs.Identity;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.DTOs.IdentityDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IBServices.IdentityManagement
{
    public interface IIdentityService
    {
        Task<RegisterResultDTO> Register(User RegisterUser);
        Task<ConfirmSMSKeyResultDTO> ConfirmKey(string PhoneNumber, string Key);
    }
}
