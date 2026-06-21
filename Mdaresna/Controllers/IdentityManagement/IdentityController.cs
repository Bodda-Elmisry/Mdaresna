using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.IdentityDTO;
using Mdaresna.Repository.IBServices.IdentityManagement;
using Mdaresna.Repository.IServices.UserManagement.Command;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Mdaresna.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Mdaresna.Controllers.IdentityManagement
{
    [Route("API/Identity")]
    public class IdentityController : Controller
    {
        private readonly IIdentityService identityService;
        private readonly IUserCommandService userCommandService;
        private readonly IUserQueryService userQueryService;
        private readonly AppDbContext context;

        public IdentityController(IIdentityService identityService,
                                  IUserCommandService userCommandService,
                                  IUserQueryService userQueryService,
                                  AppDbContext context)
        {
            this.identityService = identityService;
            this.userCommandService = userCommandService;
            this.userQueryService = userQueryService;
            this.context = context;
        }

        private Guid CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub) 
                                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;
            }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO register)
        {
            try
            {
                var user = new User
                {
                    PhoneNumber = register.PhoneNumber,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Password = string.Empty,
                    UserType = UserTypeEnum.Normal,
                    EmailConfirmed = false,
                    PhoneConfirmed = false,
                    CreateDate = DateTime.Now,
                    LastModifyDate = DateTime.Now
                };

                var registerd = await identityService.Register(user);
                return registerd.Regidterd ? Ok(user) : 
                            (string.IsNullOrEmpty(registerd.MSG) ? BadRequest("Error") :
                                            Conflict(registerd.MSG));
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ConfirmPhonNumber")]
        public async Task<IActionResult> ConfirmPhonNumber([FromBody] ConfirmPhoneDTO confirmPhone)
        {
            try
            {
                var result = await identityService.ConfirmKey(confirmPhone.PhoneNumber, confirmPhone.Key);

                return result.Confirmed ? Ok(result.User) : BadRequest(result.MSG);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SaveUserMainInfo")]
        public async Task<IActionResult> SaveUserMainInfo([FromBody] SaveUserMainInfoDTO userMainInfo)
        {
            try
            {
                if (userMainInfo == null)
                {
                    return BadRequest("UserMainInfo cannot be null");
                }

                var existingUser = await userQueryService.GetByIdAsync(userMainInfo.Id);
                if (existingUser != null && !string.IsNullOrEmpty(existingUser.Password))
                {
                    if (CurrentUserId == Guid.Empty || userMainInfo.Id != CurrentUserId)
                    {
                        return Forbid();
                    }
                }

                var user = new User
                {
                    Id = userMainInfo.Id,
                    UserName = userMainInfo.UserName,
                    FirstName = userMainInfo.FirstName,
                    LastName = userMainInfo.LastName,
                    Password = userMainInfo.Password,
                    //EncriptionKey = userMainInfo.EncriptionKey,
                    ImageUrl = userMainInfo.ImageUrl
                };
                var result = await identityService.SaveUserMainInfo(user);

                return result.Saved ? Ok(result.User) : BadRequest(result.MSG);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dTO)
        {
            try
            {
                if (dTO.Id != CurrentUserId)
                {
                    return Forbid();
                }

                var result = await identityService.ChangePassword(dTO.Id, dTO.OldPassword, dTO.NewPassword);
                return result.Saved ? Ok("Password changed") : BadRequest(result.MSG);
            }


            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword([FromBody] PhoneNumberDTO dTO)
        {
            try
            {
                var sent = await identityService.ForgetPassword(dTO.PhoneNumber);

                return sent.ConfermationKeySent ? Ok(sent.UserId) : BadRequest(sent.MSG);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddUserNewPassword")]
        public async Task<IActionResult> AddUserNewPassword([FromBody] AddUserNewPasswordDTO dTO)
        {
            try
            {
                var result = await identityService.AddUserNewPassword(dTO.UserId, dTO.Password);

                return result.PasswordChanged ? Ok("Password Changed") : BadRequest(result.MSG);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            try
            {
                var result = await identityService.Login(login.PhoneNumber, login.Password, login.SchoolId);
                return result == null ? BadRequest("Wrong phone number or password") : Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO dTO)
        {
            try
            {
                if (string.IsNullOrEmpty(dTO.RefreshToken))
                {
                    return BadRequest("Refresh token cannot be empty");
                }

                var result = await identityService.RefreshToken(dTO.RefreshToken);
                return result == null ? Unauthorized("Invalid or expired refresh token") : Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDTO dTO)
        {
            try
            {
                if (string.IsNullOrEmpty(dTO.RefreshToken))
                {
                    return BadRequest("Refresh token cannot be empty");
                }

                await identityService.Logout(dTO.RefreshToken);
                return Ok("Logged out successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("DeleteAccount")]
        public async Task<IActionResult> DeleteAccount([FromBody] UserIdDTO idDTO)
        {
            try
            {
                if (idDTO.UserId != CurrentUserId)
                {
                    return Forbid();
                }

                var user = await userQueryService.GetByIdAsync(idDTO.UserId);
                if (user == null)
                    return BadRequest("User not exist to delete");

                var isAdminForActiveSchools = await context.Schools
                    .AnyAsync(s => s.SchoolAdminId == idDTO.UserId && s.Deleted == false);

                if (isAdminForActiveSchools)
                    return Conflict("Please delete schools before delete your account");

                user.Deleted = true;

                var deleted = userCommandService.Update(user);
                return deleted ? Ok("Account deleted") : BadRequest("Error in delete account");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
