using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.DTOs.IdentityDTO;
using Mdaresna.Repository.IBServices.IdentityManagement;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace Mdaresna.Controllers.IdentityManagement
{
    [Route("API/Identity")]
    public class IdentityController : Controller
    {
        private readonly IIdentityService identityService;

        public IdentityController(IIdentityService identityService)
        {
            this.identityService = identityService;
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

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            try
            {
                var result = await identityService.Login(login.PhoneNumber, login.Password);
                return result == null ? BadRequest("Wrong phone number or password") : Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
