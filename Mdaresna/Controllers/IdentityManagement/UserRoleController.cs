using Mdaresna.Doamin.Models.Identity;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.IdentityDTO;
using Mdaresna.Repository.IServices.IdentityManagement.Command;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.IdentityManagement
{
    [Route("UserRole")]
    public class UserRoleController : Controller
    {
        private readonly IUserRoleQueryService userRoleQueryService;
        private readonly IUserRoleCommandService userRoleCommandService;

        public UserRoleController(IUserRoleQueryService userRoleQueryService,
                                  IUserRoleCommandService userRoleCommandService)
        {
            this.userRoleQueryService = userRoleQueryService;
            this.userRoleCommandService = userRoleCommandService;
        }

        [HttpPost("GetUserRoles")]
        public async Task<IActionResult> GetUserRoles([FromBody] GetUserRolesDTO dTO)
        {
            try
            {
                var userRoles = await userRoleQueryService.GetUserRolesAsync(dTO.UserId, dTO.SchoolId);
                return Ok(userRoles);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("GetUserRole")]
        public async Task<IActionResult> GetUserRole([FromBody] UserIdRoleIdDTO dTO)
        {
            try
            {
                var userRole = await userRoleQueryService.GetUserRoleAsync(dTO.UserId, dTO.RoleId);
                return Ok(userRole);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("AssignUerRoles")]
        public async Task<IActionResult> AssignUerRoles([FromBody] AssignAndRemoveUserRolesDTO dTO)
        {
            try
            {
                var added = await userRoleCommandService.Create(dTO.UserRoles);
                if(added)
                return Ok("Roles assignd to user");

                return BadRequest("Error in assign roles to user");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("RemoveUserRoles")]
        public async Task<IActionResult> RemoveUserRoles([FromBody] AssignAndRemoveUserRolesDTO dTO)
        {
            try
            {
                var unRemoved = string.Empty;
                var removed = true;
                

                foreach(var ur in dTO.UserRoles)
                {
                    var userRole = new UserRole
                    {
                        UserId = ur.UserId,
                        RoleId = ur.RoleId,
                        SchoolId = ur.SchoolId
                    };

                    removed = await userRoleCommandService.DeleteAsync(userRole);
                    if(!removed)
                        unRemoved = string.IsNullOrEmpty(unRemoved) ? unRemoved : $"{unRemoved}, {ur.RoleId.ToString()}";
                }

                if (string.IsNullOrEmpty(unRemoved))
                    return Ok("Roles removed from user");

                return BadRequest("Error in remove roles from user ");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



    }
}
