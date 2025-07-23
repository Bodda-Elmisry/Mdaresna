using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.UserManagementDTO;
using Mdaresna.Repository.IServices.UserManagement.Command;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.UserManagement
{
    [Route("UserDevice")]
    public class UserDeviceController : Controller
    {
        private readonly IUserDeviceCommandService userDeviceCommandService;
        private readonly IUserDeviceQueryService userDeviceQueryService;

        public UserDeviceController(IUserDeviceCommandService userDeviceCommandService, IUserDeviceQueryService userDeviceQueryService)
        {
            this.userDeviceCommandService = userDeviceCommandService;
            this.userDeviceQueryService = userDeviceQueryService;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] DeviceRegistrationDto dto)
        {
            try
            {
                if (await userDeviceQueryService.CheckIfUserFcmTokenExistAsync(dto.UserId, dto.FcmToken))
                {
                    return Ok("UserDevice created successfully");
                }

                if (dto == null)
                {
                    return BadRequest("UserDevice cannot be null");
                }

                var device = new UserDevice
                {
                    UserId = dto.UserId,
                    DeviceId = dto.DeviceId,
                    Platform = dto.Platform,
                    FcmToken = dto.FcmToken,
                };

                var result = userDeviceCommandService.Create(device);
                if (result)
                {
                    return Ok("UserDevice created successfully");
                }
                else
                {
                    return StatusCode(500, "Error creating UserDevice");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteUserDeviceDTO dto)
        {
            try
            {
                
                var userDevice = await userDeviceQueryService.GetByUserIdAndFcmTockenAsync(dto.UserId, dto.FcmTocken);

                if (userDevice == null)
                {
                    return BadRequest("UserDevice cannot be null");
                }


                var result = await userDeviceCommandService.DeleteAsync(userDevice);
                if (result)
                {
                    return Ok("UserDevice deleted successfully");
                }
                else
                {
                    return StatusCode(500, "Error deleting UserDevice");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
