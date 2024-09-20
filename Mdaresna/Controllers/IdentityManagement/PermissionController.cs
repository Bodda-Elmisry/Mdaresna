using Mdaresna.DTOs.IdentityDTO;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Microsoft.AspNetCore.Mvc;
using System.Security;

namespace Mdaresna.Controllers.IdentityManagement
{
    [Route("Permission")]
    public class PermissionController : Controller
    {
        private readonly IPermissionQueryService permissionQueryService;

        public PermissionController(IPermissionQueryService permissionQueryService)
        {
            this.permissionQueryService = permissionQueryService;
        }

        [HttpPost("GetPermissions")]
        public async Task<IActionResult> GetPermissions([FromBody] GetPermissionsDTO dTO)
        {
            try
            {
                var permissions = await permissionQueryService.GetPermissionsListAsync(dTO.permissionsType,
                                                                                 dTO.PageNumber);

                return Ok(permissions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }





    }
}
