using Mdaresna.Doamin.Models.Identity;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.IdentityDTO;
using Mdaresna.Repository.IServices.IdentityManagement.Command;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.IdentityManagement
{
    [Route("RolePermission")]
    [Authorize]
    public class RolePermissionController : Controller
    {
        private readonly IRolePermissionCommandService rolePermissionCommandService;
        private readonly IRolePermissionQueryService rolePermissionQueryService;
        private readonly IRoleQueryService roleQueryService;
        private readonly IPermissionQueryService permissionQueryService;
        private readonly ISchoolAccessValidator schoolAccessValidator;

        public RolePermissionController(IRolePermissionCommandService rolePermissionCommandService,
                                        IRolePermissionQueryService rolePermissionQueryService,
                                        IRoleQueryService roleQueryService,
                                        IPermissionQueryService permissionQueryService,
                                        ISchoolAccessValidator schoolAccessValidator)
        {
            this.rolePermissionCommandService = rolePermissionCommandService;
            this.rolePermissionQueryService = rolePermissionQueryService;
            this.roleQueryService = roleQueryService;
            this.permissionQueryService = permissionQueryService;
            this.schoolAccessValidator = schoolAccessValidator;
        }

        private Guid CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)
                    ? userId
                    : Guid.Empty;
            }
        }

        private bool HasPermission(string permissionKey)
        {
            return User.FindAll("permissions")
                .Select(claim => claim.Value.Split(':')[0])
                .Any(key => string.Equals(key, permissionKey, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<IActionResult?> AuthorizeChangeAsync(Role role, string applicationPermission)
        {
            if (role.SchoolRole && role.SchoolId.HasValue)
            {
                if (!HasPermission("ManageSchoolRolePermissions") ||
                    !await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, role.SchoolId.Value))
                {
                    return Forbid();
                }

                return null;
            }

            return HasPermission(applicationPermission) ? null : Forbid();
        }

        private async Task<bool> AreAssignableSchoolPermissionsAsync(IEnumerable<Guid> permissionIds)
        {
            var requestedIds = permissionIds.Distinct().ToHashSet();
            if (requestedIds.Count == 0)
            {
                return true;
            }

            var allowedIds = (await permissionQueryService.GetAllAsync())
                .Where(permission => permission.SchoolPermission &&
                                     permission.AvailableForSchoolCustomRoles &&
                                     !permission.Deleted)
                .Select(permission => permission.Id)
                .ToHashSet();
            return requestedIds.IsSubsetOf(allowedIds);
        }

        [HttpPost("AssignPermissionsToRole")]
        public async Task<IActionResult> AssignPermissionsToRole([FromBody] AssignAndRemoveRolePermissionsDTO dTO)
        {
            try
            {
                var role = await roleQueryService.GetByIdAsync(dTO.RoleId);
                if (role == null)
                    return NotFound("Role not found");

                var authorizationError = await AuthorizeChangeAsync(role, "AddPermissionsToRole");
                if (authorizationError != null)
                    return authorizationError;

                var permissionIds = dTO.Permissions?.Distinct().ToList() ?? new List<Guid>();
                if (role.SchoolRole && role.SchoolId.HasValue &&
                    !await AreAssignableSchoolPermissionsAsync(permissionIds))
                    return BadRequest("One or more permissions cannot be assigned to a custom school role");

                var rolePermissions = permissionIds.Select(p => new RolePermission
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
                var role = await roleQueryService.GetByIdAsync(dTO.RoleId);
                if (role == null)
                    return NotFound("Role not found");

                var authorizationError = await AuthorizeChangeAsync(role, "RemovePermissionsToRole");
                if (authorizationError != null)
                    return authorizationError;

                var unRemoved = string.Empty;
                var removed = true;

                foreach (var permission in dTO.Permissions?.Distinct() ?? Enumerable.Empty<Guid>())
                {
                    var rolePermission = new RolePermission
                    {
                        CreateDate = DateTime.Now,
                        LastModifyDate = DateTime.Now,
                        PermissionId = permission,
                        RoleId = dTO.RoleId
                    };

                    removed = await rolePermissionCommandService.DeleteAsync(rolePermission);

                    if(!removed)
                        unRemoved = string.IsNullOrEmpty(unRemoved) ? unRemoved : $"{unRemoved}, {permission.ToString()}";

                }


                return string.IsNullOrEmpty(unRemoved) ? Ok("Permissions removed") : BadRequest("Error in remove permissions");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateSchoolRolePermissions")]
        public async Task<IActionResult> UpdateSchoolRolePermissions([FromBody] AssignAndRemoveRolePermissionsDTO dTO)
        {
            try
            {
                var role = await roleQueryService.GetByIdAsync(dTO.RoleId);
                if (role == null)
                    return NotFound("Role not found");
                if (!role.SchoolRole || !role.SchoolId.HasValue)
                    return BadRequest("Only custom school roles can be managed through this endpoint");

                var authorizationError = await AuthorizeChangeAsync(role, "ManageSchoolRolePermissions");
                if (authorizationError != null)
                    return authorizationError;

                var permissionIds = dTO.Permissions?.Distinct().ToList() ?? new List<Guid>();
                if (!await AreAssignableSchoolPermissionsAsync(permissionIds))
                    return BadRequest("One or more permissions cannot be assigned to a custom school role");

                var updated = await rolePermissionCommandService
                    .ReplaceRolePermissionsAsync(role.Id, permissionIds);
                return updated
                    ? Ok("School role permissions updated")
                    : BadRequest("Error updating school role permissions");
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
                var role = await roleQueryService.GetByIdAsync(dTO.RoleId);
                if (role == null)
                    return NotFound("Role not found");

                if (role.SchoolRole && role.SchoolId.HasValue)
                {
                    var canView = HasPermission("ViewSchoolRoles") ||
                                  HasPermission("EditSchoolRole") ||
                                  HasPermission("ManageSchoolRolePermissions");
                    if (!canView ||
                        !await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, role.SchoolId.Value))
                        return Forbid();
                }
                else if (!HasPermission("ViewPermissionSettings"))
                {
                    return Forbid();
                }

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
