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
        Task<SaveUserMainInfoResultDTO> SaveUserMainInfo(User userInfo);
        Task<LoginResultDTO> Login(string PhoneNumber, string Password);
        Task<ChangePasswordResultDTO> ChangePassword(Guid userId, string oldPassword, string newPassword);
        Task<ForgetPasseordResultDTO> ForgetPassword(string phoneNumber);
        Task<AddUserNewPasswordResultDTO> AddUserNewPassword(Guid userId, string Password);
    }
}
