using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.IdentityDTO;
using Mdaresna.Repository.IBServices.IdentityManagement;
using Mdaresna.Repository.IServices.UserManagement.Command;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace Mdaresna.Controllers.IdentityManagement
{
    [Route("API/Identity")]
    public class IdentityController : Controller
    {
        private readonly IIdentityService identityService;
        private readonly IUserCommandService userCommandService;
        private readonly IUserQueryService userQueryService;

        public IdentityController(IIdentityService identityService, IUserCommandService userCommandService, IUserQueryService userQueryService)
        {
            this.identityService = identityService;
            this.userCommandService = userCommandService;
            this.userQueryService = userQueryService;
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

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dTO)
        {
            try
            {
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

        [HttpPost("DeleteAccount")]
        public async Task<IActionResult> DeleteAccount([FromBody] UserIdDTO idDTO)
        {
            try
            {
                var user = await userQueryService.GetByIdAsync(idDTO.UserId);
                if (user == null)
                    return BadRequest("User not exist to delete");

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
