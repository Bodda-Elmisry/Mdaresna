using Mdaresna.Doamin.Models.Identity;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.IdentityDTO;
using Mdaresna.Repository.IServices.IdentityManagement.Command;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.IdentityManagement
{
    [Route("UserPermission")]
    public class UserPermissionController : Controller
    {
        private readonly IUserPermissionCommandService userPermissionCommandService;
        private readonly IUserPermissionQueryService userPermissionQueryService;

        public UserPermissionController(IUserPermissionCommandService userPermissionCommandService,
                                        IUserPermissionQueryService userPermissionQueryService)
        {
            this.userPermissionCommandService = userPermissionCommandService;
            this.userPermissionQueryService = userPermissionQueryService;
        }

        [HttpPost("GetUserPermissions")]
        public async Task<IActionResult> GetUserPermissions([FromBody] UserIdDTO dTO)
        {
            try
            {
                var permissions = await userPermissionQueryService.GetUserPermissionsView(dTO.UserId);

                return Ok(permissions);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AssignPermissionsToUser")]
        public async Task<IActionResult> AssignPermissionsToUser([FromBody] AssignAndRemoveUserPermissionsDTO dto)
        {
            try
            {
                var userPermissions = dto.UserPermissions.Select(u => new UserPermission
                {
                    UserId = u.UserId,
                    PermissionId = u.PermissionId,
                    SchoolId = u.SchoolId,
                    CreateDate = DateTime.Now,
                    LastModifyDate = DateTime.Now
                });

                var added = await userPermissionCommandService.Create(userPermissions);

                return added ? Ok("Permissions added") : BadRequest("Error in adding permissions");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("RemovePermissionsFromUser")]
        public async Task<IActionResult> RemovePermissionsFromUser([FromBody] AssignAndRemoveUserPermissionsDTO dto)
        {
            try
            {
                var notRemoved = string.Empty;
                var removed = true;

                foreach(var up in dto.UserPermissions)
                {
                    var userPermission = await userPermissionQueryService.GetUserPermissionByID(up.PermissionId, up.SchoolId, up.UserId);

                    removed = await userPermissionCommandService.DeleteAsync(userPermission);

                    notRemoved = string.IsNullOrEmpty(notRemoved) ? up.PermissionId.ToString() : $", {up.PermissionId.ToString()}" ;
                }

                return string.IsNullOrEmpty(notRemoved) ? Ok("Permissions removed from user") : BadRequest("Error in remove some permissions from user");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



    }
}
