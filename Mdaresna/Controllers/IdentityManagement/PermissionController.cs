using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.IdentityDTO;
using Mdaresna.Repository.IServices.IdentityManagement.Command;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Microsoft.AspNetCore.Mvc;
using System.Security;

namespace Mdaresna.Controllers.IdentityManagement
{
    [Route("Permission")]
    public class PermissionController : Controller
    {
        private readonly IPermissionQueryService permissionQueryService;
        private readonly IPermissionCommandService permissionCommandService;

        public PermissionController(IPermissionQueryService permissionQueryService,
                                    IPermissionCommandService permissionCommandService)
        {
            this.permissionQueryService = permissionQueryService;
            this.permissionCommandService = permissionCommandService;
        }

        [HttpPost("GetPermissions")]
        public async Task<IActionResult> GetPermissions([FromBody] GetPermissionsDTO dTO)
        {
            try
            {
                var permissions = await permissionQueryService.GetPermissionsListAsync(dTO.permissionsType,
                                                                                 dTO.PageNumber,
                                                                                 dTO.PermissionName,
                                                                                 dTO.UserId);

                return Ok(permissions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("TogglePermissionsMappingToClassroom")]
        public async Task<IActionResult> TogglePermissionsMappingToClassroom([FromBody] PermissionIdDTO dTO)
        {
            try
            {
                var permission = await permissionQueryService.GetByIdAsync(dTO.PermissionId);

                permission.AllowedToMapToClassroom = !permission.AllowedToMapToClassroom;

                var updated = permissionCommandService.Update(permission);


                return updated ? Ok("Toggle Completed") : BadRequest("Error in toggle");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }





    }
}
