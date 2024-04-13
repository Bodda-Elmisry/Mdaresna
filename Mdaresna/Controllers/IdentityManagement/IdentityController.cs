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
        public async Task<IActionResult> Register(RegisterDTO register)
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
                    PhoneConfirmed = false
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
        public async Task<IActionResult> ConfirmPhonNumber(ConfirmPhoneDTO confirmPhone)
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
    }
}
