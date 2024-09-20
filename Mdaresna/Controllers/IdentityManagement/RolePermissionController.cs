using Mdaresna.Doamin.Models.Identity;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.IdentityDTO;
using Mdaresna.Repository.IServices.IdentityManagement.Command;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Microsoft.AspNetCore.Mvc;
using System.Security;

namespace Mdaresna.Controllers.IdentityManagement
{
    [Route("RolePermission")]
    public class RolePermissionController : Controller
    {
        private readonly IRolePermissionCommandService rolePermissionCommandService;
        private readonly IRolePermissionQueryService rolePermissionQueryService;

        public RolePermissionController(IRolePermissionCommandService rolePermissionCommandService,
                                        IRolePermissionQueryService rolePermissionQueryService)
        {
            this.rolePermissionCommandService = rolePermissionCommandService;
            this.rolePermissionQueryService = rolePermissionQueryService;
        }

        [HttpPost("AssignPermissionsToRole")]
        public async Task<IActionResult> AssignPermissionsToRole([FromBody] AssignAndRemoveRolePermissionsDTO dTO)
        {
            try
            {
                var rolePermissions = dTO.Permissions.Select(p => new RolePermission
                {
                    CreateDate = DateTime.Now,
                    LastModifyDate = DateTime.Now,
                    PermissionId = p,
                    RoleId = dTO.RoleId
                });

                var removed = await rolePermissionCommandService.Create(rolePermissions);

                return removed ? Ok("Permissions assigned") : BadRequest("Error in assign permissions");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("RemovePermissionsFromRole")]
        public async Task<IActionResult> RemovePermissionsFromRole([FromBody] AssignAndRemoveRolePermissionsDTO dTO)
        {
            try
            {
                var rolePermissions = dTO.Permissions.Select(p => new RolePermission
                {
                    CreateDate = DateTime.Now,
                    LastModifyDate = DateTime.Now,
                    PermissionId = p,
                    RoleId = dTO.RoleId
                });

                var removed = await rolePermissionCommandService.DeleteAsync(rolePermissions);

                return removed ? Ok("Permissions removed") : BadRequest("Error in remove permissions");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetRolePermissions")]
        public async Task<IActionResult> GetRolePermissions([FromBody] RoleIdDTO dTO)
        {
            try
            {
                var rolePermissions = await rolePermissionQueryService.GetRolePermissions(dTO.RoleId);
                return Ok(rolePermissions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }






    }
}
