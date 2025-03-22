using Mdaresna.Doamin.DTOs.Identity;
using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.DTOs.IdentityDTO;
using Mdaresna.Repository.Helpers;
using Mdaresna.Repository.IBServices.IdentityManagement;
using Mdaresna.Repository.IRepositories.IdentityManagement.Command;
using Mdaresna.Repository.IServices.IdentityManagement.Command;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IServices.SettingsManagement.Query;
using Mdaresna.Repository.IServices.UserManagement.Command;
using Mdaresna.Repository.IServices.UserManagement.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.BServices.IdentityManagement
{
    public class IdentityService : IIdentityService
    {
        private readonly IUserCommandService userCommandService;
        private readonly IUserQueryService userQueryService;
        private readonly ISMSProviderQueryService sMSProviderQueryService;
        private readonly IUserPermissionQueryService userPermissionQueryService;
        private readonly IUserRoleQueryService userRoleQueryService;
        private readonly IUserRoleCommandService userRoleCommandService;
        private readonly IRoleQueryService roleQueryService;
        private readonly ISchoolQueryService schoolQueryService;

        public IdentityService(IUserCommandService userCommandService,
                                IUserQueryService userQueryService,
                                ISMSProviderQueryService sMSProviderQueryService,
                                IUserPermissionQueryService userPermissionQueryService,
                                IUserRoleQueryService userRoleQueryService,
                                IUserRoleCommandService userRoleCommandService,
                                IRoleQueryService roleQueryService,
                                ISchoolQueryService schoolQueryService)
        {
            this.userCommandService = userCommandService;
            this.userQueryService = userQueryService;
            this.sMSProviderQueryService = sMSProviderQueryService;
            this.userPermissionQueryService = userPermissionQueryService;
            this.userRoleQueryService = userRoleQueryService;
            this.userRoleCommandService = userRoleCommandService;
            this.roleQueryService = roleQueryService;
            this.schoolQueryService = schoolQueryService;
        }

        public async Task<RegisterResultDTO> Register(User RegisterUser)
        {
            var result = new RegisterResultDTO { MSG = string.Empty };

            var user = await userQueryService.GetUserByPhoneNumber(RegisterUser.PhoneNumber);
            if (user != null && !string.IsNullOrEmpty(user.Password))
            {
                result.MSG = "User Already Exist";
                result.Regidterd = false;
                return result;
            }

            RegisterUser.PhoneConfirmationCode = SMSHelper.GenerateConfirmationKey();
            RegisterUser.EncriptionKey = UserHelper.GenerateEncriptionKey(32);

            if (user == null)
            {
                result.Regidterd = userCommandService.Create(RegisterUser);
            }
            else
            {
                RegisterUser.Id = user.Id;
                result.Regidterd = true;
            }
            var addStanderdRole = await AddStanderdRoleToUser(RegisterUser.Id);
            result.MSG = await SendConferamtionKey(RegisterUser);
            return result;
        }

        private async Task<bool> AddStanderdRoleToUser(Guid UserId)
        {
            var result = false;
            var standerdRole = await roleQueryService.GetStanderdRole();

            if( standerdRole == null ) 
            { 
                return false;
            }

            var standerdUserRole = new UserRole { UserId = UserId, RoleId = standerdRole.Id };

            result = await userRoleQueryService.CheckRoleExist(standerdUserRole);

            if (!result)
            {
                userRoleCommandService.Create(standerdUserRole);
                result = true;
            }

            return result;
        }

        private async Task<string> SendConferamtionKey(User user)
        {
            var smsProvider = await sMSProviderQueryService.GetFirstActive();
            string MSG;
            MSG = await SMSHelper.SendConfirmationKey(smsProvider, user);
            return MSG;
        }

        public async Task<ConfirmSMSKeyResultDTO> ConfirmKey(string PhoneNumber, string Key)
        {
            var result = new ConfirmSMSKeyResultDTO
            {
                Confirmed = false,
                MSG = "Wrong Confirmation Key",
                User = null
            };

            var user = await userQueryService.GetUserByPhoneNumberAndConfirmationKey(PhoneNumber, Key);
            if (user != null)
            {
                user.PhoneConfirmed = true;
                user.PhoneConfirmationCode = null;
                result.Confirmed = userCommandService.Update(user);
                if(result.Confirmed)
                {
                    result.MSG = "Number Confirmed";
                    result.User = user;
                }
            }

            return result;
        }

        public async Task<SaveUserMainInfoResultDTO> SaveUserMainInfo(User userInfo)
        {
            var result = new SaveUserMainInfoResultDTO
            {
                Saved = false,
                MSG = "Error",
                User = null
            };

            var user = await userQueryService.GetByIdAsync(userInfo.Id);

            if (user != null)
            {
                user.UserName = userInfo.UserName;
                user.FirstName = userInfo.FirstName;
                user.LastName = userInfo.LastName;
                user.Password = UserHelper.EncryptPassword(userInfo.Password, user.EncriptionKey);
                user.ImageUrl = userInfo.ImageUrl;

                result.Saved = userCommandService.Update(user);

            }


            if (result.Saved)
            {
                result.MSG = "Info Saved";
                result.User = user;
            }

            return result;
        }

        public async Task<ChangePasswordResultDTO> ChangePassword(Guid userId, string oldPassword, string newPassword)
        {
            var result = new ChangePasswordResultDTO
            {
                Saved = false,
                MSG = ""
            };
            var user = await userQueryService.GetByIdAsync(userId);

            if(user == null)
            {
                result.MSG = "Can't fiend user";
            }
            else
            {
                var encreptedOldPass = UserHelper.EncryptPassword(oldPassword, user.EncriptionKey);

                if (encreptedOldPass != user.Password)
                    result.MSG = "Wrong old password";
                else
                {
                    var encrptedPass = UserHelper.EncryptPassword(newPassword, user.EncriptionKey);
                    user.Password = encrptedPass;
                    var userUpdated = userCommandService.Update(user);
                    if(userUpdated)
                    {
                        result.Saved = true;
                        result.MSG = string.Empty;
                    }
                    else
                    {
                        result.MSG = "Error in changing passeord";
                    }
                }
            }

            return result;

        }

        public async Task<ForgetPasseordResultDTO> ForgetPassword(string phoneNumber)
        {
            var result = new ForgetPasseordResultDTO
            {
                ConfermationKeySent = false,
                MSG = "THis phone not regester",
                UserId = null
            };
            var user = await userQueryService.GetUserByPhoneNumber(phoneNumber);

            if(user != null)
            {
                var confirmationKey = await this.SendConferamtionKey(user);
                if(!string.IsNullOrEmpty(confirmationKey))
                {
                    result.ConfermationKeySent = true;
                    result.MSG = string.Empty;
                    result.UserId = user.Id;
                }
            }

            return result;
        }

        public async Task<AddUserNewPasswordResultDTO> AddUserNewPassword(Guid userId, string Password)
        {
            var result = new AddUserNewPasswordResultDTO
            {
                PasswordChanged = false,
                MSG = "password not changed"
            };
            var user = await userQueryService.GetByIdAsync(userId);

            if(user != null)
            {
                user.Password= UserHelper.EncryptPassword(Password, user.EncriptionKey);

                var updated = userCommandService.Update(user);

                if(updated)
                {
                    result.MSG = string.Empty;
                    result.PasswordChanged = true;
                }
            }


            return result;

        }

        public async Task<LoginResultDTO> Login(string PhoneNumber, string Password, Guid? schoolId)
        {
            var user = await userQueryService.GetUserByPhoneNumber(PhoneNumber);

            if (user != null && user.Id != Guid.Empty)
            {
                //var decp = UserHelper.DecryptPassword(user.Password, user.EncriptionKey);
                var encriptedPassword = UserHelper.EncryptPassword(Password, user.EncriptionKey);
                user = user.Password == encriptedPassword ? user : null;

                 
            }
            var result = (user == null || user.Id == Guid.Empty) ? null 
                            : await GetUserInfo(user, schoolId);
            
            return result;
        }

        private async Task<LoginResultDTO> GetUserInfo(User user, Guid? schoolId)
        {
            var userSchools = await schoolQueryService.GetUserSchools(user.Id);
            var firstSchool = schoolId ?? (userSchools.Any() ? userSchools.FirstOrDefault().Id : null);
            var permissions = await userPermissionQueryService.GetUserPermissionsView(user.Id, firstSchool == null ? null : firstSchool);

            return new LoginResultDTO
            {
                LogedinUser = user,
                Permissions = permissions.Select(x => new PermissionClasseroomsResultDTO
                {
                    PermissionKey = x.PermissionKey,
                    Classrooms = x.Classrooms
                }),
                Schools = userSchools,
            };
        }

    }
}
