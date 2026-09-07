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
using Mdaresna.Repository.IServices.SettingsManagement.Command;
using Mdaresna.Repository.IServices.UserManagement.Command;
using Mdaresna.Repository.IServices.UserManagement.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

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
        private readonly ISchoolEmployeeQueryService schoolEmployeeQueryService;
        private readonly ISchoolTeacherQueryService schoolTeacherQueryService;
        private readonly IConfiguration configuration;
        private readonly ISMSLogCommandService smsLogCommandService;
        private readonly IUserRefreshTokenCommandService userRefreshTokenCommandService;
        private readonly IUserRefreshTokenQueryService userRefreshTokenQueryService;

        public IdentityService(IUserCommandService userCommandService,
                                IUserQueryService userQueryService,
                                ISMSProviderQueryService sMSProviderQueryService,
                                IUserPermissionQueryService userPermissionQueryService,
                                IUserRoleQueryService userRoleQueryService,
                                IUserRoleCommandService userRoleCommandService,
                                IRoleQueryService roleQueryService,
                                ISchoolQueryService schoolQueryService,
                                ISchoolEmployeeQueryService schoolEmployeeQueryService,
                                ISchoolTeacherQueryService schoolTeacherQueryService,
                                ISMSLogCommandService smsLogCommandService,
                                IConfiguration configuration,
                                IUserRefreshTokenCommandService userRefreshTokenCommandService,
                                IUserRefreshTokenQueryService userRefreshTokenQueryService)
        {
            this.userCommandService = userCommandService;
            this.userQueryService = userQueryService;
            this.sMSProviderQueryService = sMSProviderQueryService;
            this.userPermissionQueryService = userPermissionQueryService;
            this.userRoleQueryService = userRoleQueryService;
            this.userRoleCommandService = userRoleCommandService;
            this.roleQueryService = roleQueryService;
            this.schoolQueryService = schoolQueryService;
            this.schoolEmployeeQueryService = schoolEmployeeQueryService;
            this.schoolTeacherQueryService = schoolTeacherQueryService;
            this.smsLogCommandService = smsLogCommandService;
            this.configuration = configuration;
            this.userRefreshTokenCommandService = userRefreshTokenCommandService;
            this.userRefreshTokenQueryService = userRefreshTokenQueryService;
        }

        public async Task<RegisterResultDTO> Register(User RegisterUser)
        {
            var result = new RegisterResultDTO { MSG = string.Empty };

            string? plainKey = null;
            var user = await userQueryService.GetUserByPhoneNumber(RegisterUser.PhoneNumber);
            if (user != null)
            {
                if (user.PhoneConfirmed)
                {
                    result.MSG = "User Already Exist. Please login or request a password change.";
                    result.Regidterd = false;
                    return result;
                }
                else
                {
                    // User exists but phone is not confirmed. Generate a new OTP and update.
                    plainKey = SMSHelper.GenerateConfirmationKey();
                    user.PhoneConfirmationCode = UserHelper.HashConfirmationCode(plainKey);
                    userCommandService.Update(user);

                    RegisterUser.Id = user.Id;
                    RegisterUser.PhoneConfirmationCode = user.PhoneConfirmationCode;
                    RegisterUser.EncriptionKey = user.EncriptionKey;
                    result.Regidterd = true;
                }
            }
            else
            {
                // New user
                plainKey = SMSHelper.GenerateConfirmationKey();
                RegisterUser.PhoneConfirmationCode = UserHelper.HashConfirmationCode(plainKey);
                RegisterUser.EncriptionKey = UserHelper.GenerateEncriptionKey(32);
                result.Regidterd = userCommandService.Create(RegisterUser);
            }

            if (result.Regidterd && plainKey != null)
            {
                var addStanderdRole = await AddStanderdRoleToUser(RegisterUser.Id);
                result.MSG = await SendConferamtionKey(RegisterUser, plainKey);
            }

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

        private async Task<string> SendConferamtionKey(User user, string plainKey)
        {
            var smsProvider = await sMSProviderQueryService.GetFirstActive();
            string MSG;
            MSG = await SMSHelper.SendConfirmationKey(smsProvider, user, plainKey);
            var message = SMSHelper.BuildConfirmationMessage(user, plainKey);
            smsLogCommandService.Create(new Mdaresna.Doamin.Models.SettingsManagement.SMSLog
            {
                SMSProviderId = smsProvider?.Id,
                PhoneNumber = user.PhoneNumber,
                Message = message,
                Response = MSG,
                IsSuccess = !string.IsNullOrEmpty(MSG) &&
                            !MSG.StartsWith("SOMETHING WENT AWRY", StringComparison.OrdinalIgnoreCase)
            });
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

            var user = await userQueryService.GetUserByPhoneNumber(PhoneNumber);
            if (user != null && !string.IsNullOrEmpty(user.PhoneConfirmationCode))
            {
                var hashedInputKey = UserHelper.HashConfirmationCode(Key);
                if (user.PhoneConfirmationCode == hashedInputKey)
                {
                    user.PhoneConfirmed = true;
                    user.PhoneConfirmationCode = null;
                    result.Confirmed = userCommandService.Update(user);
                    if(result.Confirmed)
                    {
                        user.Token = GenerateTempToken(user);
                        result.MSG = "Number Confirmed";
                        result.User = user;
                    }
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

                        try
                        {
                            var activeTokens = (await userRefreshTokenQueryService.GetAllAsync())
                                               .Where(t => t.UserId == userId && !t.IsRevoked);
                            foreach (var token in activeTokens)
                            {
                                token.IsRevoked = true;
                                userRefreshTokenCommandService.Update(token);
                            }
                        }
                        catch
                        {
                            // Soft failure: do not block password update if token revocation fails
                        }
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
                var plainKey = SMSHelper.GenerateConfirmationKey();
                user.PhoneConfirmationCode = UserHelper.HashConfirmationCode(plainKey);
                userCommandService.Update(user);

                var confirmationKey = await this.SendConferamtionKey(user, plainKey);
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

                    try
                    {
                        var activeTokens = (await userRefreshTokenQueryService.GetAllAsync())
                                           .Where(t => t.UserId == userId && !t.IsRevoked);
                        foreach (var token in activeTokens)
                        {
                            token.IsRevoked = true;
                            userRefreshTokenCommandService.Update(token);
                        }
                    }
                    catch
                    {
                        // Soft failure
                    }
                }
            }


            return result;

        }

        public async Task<LoginResultDTO?> Login(string loginIdentifier, string Password, Guid? schoolId)
        {
            if (string.IsNullOrWhiteSpace(loginIdentifier) || string.IsNullOrEmpty(Password))
                return null;

            var candidates = await userQueryService.GetUsersByLoginIdentifier(loginIdentifier);
            var matchingUsers = candidates
                .Where(user => user.Id != Guid.Empty &&
                               user.Password == UserHelper.EncryptPassword(Password, user.EncriptionKey))
                .Take(2)
                .ToList();

            // Never choose an arbitrary account if legacy data contains a duplicate identifier.
            if (matchingUsers.Count != 1)
                return null;

            return await GetUserInfo(matchingUsers[0], schoolId);
        }

        private async Task<LoginResultDTO> GetUserInfo(User user, Guid? schoolId)
        {
            var userSchools = await schoolQueryService.GetUserSchools(user.Id, true);
            var firstSchool = schoolId ?? (userSchools.Any() ? userSchools.FirstOrDefault().Id : null);
            var permissions = await userPermissionQueryService.GetUserPermissionsView(user.Id, firstSchool == null ? null : firstSchool);
            var employee = await schoolEmployeeQueryService.IsExist(firstSchool ?? Guid.NewGuid(), user.Id);
            var teacher = await schoolTeacherQueryService.isExist(firstSchool ?? Guid.NewGuid(), user.Id);

            // Generate JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSection = configuration.GetSection("Jwt");
            var keyBytes = Encoding.UTF8.GetBytes(jwtSection["Key"] ?? "MdaresnaAPISecretKeyMustBeVeryLong32Chars!");

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty),
                new Claim("user_type", ((int)user.UserType).ToString()),
            };

            if (firstSchool != null)
            {
                claims.Add(new Claim("school_id", firstSchool.ToString()));
            }

            foreach (var perm in permissions)
            {
                var val = perm.PermissionKey;
                if (perm.Classrooms != null && perm.Classrooms.Any())
                {
                    val += ":" + string.Join(",", perm.Classrooms);
                }
                claims.Add(new Claim("permissions", val));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.TryParse(jwtSection["DurationInMinutes"], out var min) ? min : 1440),
                Issuer = jwtSection["Issuer"],
                Audience = jwtSection["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenObj = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(tokenObj);

            var refreshTokenString = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString();

            var refreshToken = new UserRefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenString,
                ExpiryDate = DateTime.Now.AddDays(7),
                IsRevoked = false
            };
            userRefreshTokenCommandService.Create(refreshToken);

            return new LoginResultDTO
            {
                LogedinUser = user,
                Schools = userSchools,
                IsEmployee = employee,
                IsTeacher = teacher,
                Token = tokenString,
                RefreshToken = refreshTokenString
            };

        }

        public async Task<LoginResultDTO?> RefreshToken(string token, Guid? schoolId = null)
        {
            var storedToken = await userRefreshTokenQueryService.GetByTokenAsync(token);
            if (storedToken == null || storedToken.ExpiryDate < DateTime.Now || storedToken.IsRevoked)
            {
                return null;
            }

            var user = await userQueryService.GetByIdAsync(storedToken.UserId);
            if (user == null || user.Deleted)
            {
                return null;
            }

            // Revoke old token
            storedToken.IsRevoked = true;
            userRefreshTokenCommandService.Update(storedToken);

            // Generate new token & new refresh token
            return await GetUserInfo(user, schoolId);
        }

        public async Task<bool> Logout(string token)
        {
            var storedToken = await userRefreshTokenQueryService.GetByTokenAsync(token);
            if (storedToken != null)
            {
                storedToken.IsRevoked = true;
                return userRefreshTokenCommandService.Update(storedToken);
            }
            return false;
        }

        private string GenerateTempToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSection = configuration.GetSection("Jwt");
            var keyBytes = Encoding.UTF8.GetBytes(jwtSection["Key"] ?? "MdaresnaAPISecretKeyMustBeVeryLong32Chars!");

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty),
                new Claim("user_type", ((int)user.UserType).ToString()),
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15), // Short-lived (15 minutes) token for registration/reset completion
                Issuer = jwtSection["Issuer"],
                Audience = jwtSection["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenObj = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(tokenObj);
        }

    }
}
