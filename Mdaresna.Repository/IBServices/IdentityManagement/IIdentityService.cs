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
        Task<ConfirmSMSKeyResultDTO> ConfirmKey(Guid challengeId, string key);
        Task<SaveUserMainInfoResultDTO> SaveUserMainInfo(User userInfo);
        Task<LoginResultDTO?> Login(string loginIdentifier, string Password, Guid? schoolId);
        Task<ChangePasswordResultDTO> ChangePassword(Guid userId, string oldPassword, string newPassword);
        Task<ForgetPasseordResultDTO> ForgetPassword(string phoneNumber);
        Task<AddUserNewPasswordResultDTO> AddUserNewPassword(Guid userId, string Password);
        Task<LoginResultDTO?> RefreshToken(string token, Guid? schoolId = null);
        Task<bool> Logout(string token);
    }
}
