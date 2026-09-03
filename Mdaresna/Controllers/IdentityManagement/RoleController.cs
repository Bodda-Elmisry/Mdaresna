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
    [Route("Role")]
    [Authorize]
    public class RoleController : Controller
    {
        private readonly IRoleCommandService roleCommandService;
        private readonly IRoleQueryService roleQueryService;
        private readonly IUserRoleQueryService userRoleQueryService;
        private readonly IPermissionQueryService permissionQueryService;
        private readonly ISchoolAccessValidator schoolAccessValidator;

        public RoleController(IRoleCommandService roleCommandService,
                              IRoleQueryService roleQueryService, 
                              IUserRoleQueryService userRoleQueryService,
                              IPermissionQueryService permissionQueryService,
                              ISchoolAccessValidator schoolAccessValidator)
        {
            this.roleCommandService = roleCommandService;
            this.roleQueryService = roleQueryService;
            this.userRoleQueryService = userRoleQueryService;
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

        private async Task<bool> AreAssignableSchoolPermissionsAsync(IEnumerable<Guid> permissionIds)
        {
            var requestedIds = permissionIds.Distinct().ToHashSet();
            if (requestedIds.Count == 0)
                return true;

            var allowedIds = (await permissionQueryService.GetAllAsync())
                .Where(permission => permission.SchoolPermission &&
                                     permission.AvailableForSchoolCustomRoles &&
                                     !permission.Deleted)
                .Select(permission => permission.Id)
                .ToHashSet();
            return requestedIds.IsSubsetOf(allowedIds);
        }

        private async Task<IActionResult?> AuthorizeRoleWriteAsync(
            Role role,
            string schoolPermission,
            string applicationPermission)
        {
            if (role.SchoolRole && role.SchoolId.HasValue)
            {
                if (!HasPermission(schoolPermission) ||
                    !await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, role.SchoolId.Value))
                    return Forbid();

                return null;
            }

            return HasPermission(applicationPermission) ? null : Forbid();
        }

        [HttpPost("GetRolesList")]
        public async Task<IActionResult> GetRolesList([FromBody] GetRolesDTO dTO)
        {
            try
            {
                var ignoredRoles = new List<Guid>();
                ignoredRoles.Add(Guid.Parse("92D00B28-9D25-4BD2-A587-6C22A3A07A92")); //Standerd
                ignoredRoles.Add(Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73")); //school manager
                //ignoredRoles.Add(Guid.Parse("10620C5F-37FE-4D18-996F-915ECE8893F1")); //school teacher
                var roles = await roleQueryService.GetRolesAsync(dTO.Type, dTO.Name, dTO.Activation, dTO.Description, ignoredRoles, dTO.SchoolId);

                return Ok(roles);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetRoleByID")]
        public async Task<IActionResult> GetRoleById([FromBody] RoleIdDTO dTO)
        {
            try
            {
                var role = await roleQueryService.GetByIdAsync(dTO.RoleId);

                return Ok(role);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddRole")]
        public async Task<IActionResult> AddRoles([FromBody] CreateRoleDTO dTO)
        {
            try
            {
                var permissionIds = dTO.Permissions?.Distinct().ToList() ?? new List<Guid>();
                var isCustomSchoolRole = dTO.IsSchoolRole && dTO.SchoolId.HasValue;

                if (isCustomSchoolRole)
                {
                    if (!HasPermission("CreateSchoolRole") ||
                        !await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, dTO.SchoolId!.Value))
                        return Forbid();

                    if (permissionIds.Count > 0 && !HasPermission("ManageSchoolRolePermissions"))
                        return Forbid();

                    if (!await AreAssignableSchoolPermissionsAsync(permissionIds))
                        return BadRequest("One or more permissions cannot be assigned to a custom school role");
                }
                else if (!HasPermission("CreateRole"))
                {
                    return Forbid();
                }

                var existingRolesWithTheSameName = await roleQueryService.GetRolesAsync(
                    dTO.IsSchoolRole ? 1 : 2,
                    dTO.Name,
                    true,
                    null,
                    null,
                    dTO.SchoolId
                    );

                if (existingRolesWithTheSameName != null && existingRolesWithTheSameName.Count() > 0)
                    return Conflict("Role already exist");

                var role = new Role
                {
                    Name = dTO.Name,
                    Description = dTO.Description,
                    Active = dTO.Active,
                    AdminRole = !dTO.IsSchoolRole,
                    SchoolRole = dTO.IsSchoolRole,
                    SchoolId = dTO.IsSchoolRole ? dTO.SchoolId : null
                };

                var added = await roleCommandService.Create(role, permissionIds);

                if (!added)
                    return BadRequest("Error in adding role");

                return Ok(role);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateRole")]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleDTO dTO)
        {
            try
            {
                var role = await roleQueryService.GetByIdAsync(dTO.RoleId);

                if (role == null)
                    return BadRequest("There is no role to update");

                var authorizationError = await AuthorizeRoleWriteAsync(
                    role,
                    "EditSchoolRole",
                    "EditRole");
                if (authorizationError != null)
                    return authorizationError;

                role.Name = dTO.Name;
                role.Description = dTO.Description;

                var updated = roleCommandService.Update(role);

                if (!updated)
                    return BadRequest("Error in update");

                return Ok(role);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ChangeRoleActivation")]
        public async Task<IActionResult> ChangRoleActivation([FromBody] ChangeRoleActivationDTO dTO)
        {
            try
            {
                var role = await roleQueryService.GetByIdAsync(dTO.RoleId);

                if (role == null)
                    return BadRequest("There is no role to change activations");

                var authorizationError = await AuthorizeRoleWriteAsync(
                    role,
                    "EditSchoolRole",
                    "EditRole");
                if (authorizationError != null)
                    return authorizationError;

                if (role.Active == dTO.Active)
                    return Ok("Role activation changed");

                role.Active = dTO.Active;

                var updated = roleCommandService.Update(role);

                if (!updated)
                    return BadRequest("Error in changing activation");

                return Ok("Role activation changed");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("DeleteRole")]
        public async Task<IActionResult> DeleteRole([FromBody] RoleIdDTO dto)
        {
            try
            {
                var existingRole = await roleQueryService.GetByIdAsync(dto.RoleId);
                if (existingRole == null)
                    return NotFound("Role not found");

                var authorizationError = await AuthorizeRoleWriteAsync(
                    existingRole,
                    "DeleteSchoolRole",
                    "DeleteSecurityGroup");
                if (authorizationError != null)
                    return authorizationError;

                var userRoles = await userRoleQueryService.GetRoleUsersAsync(
                    dto.RoleId,
                    existingRole.SchoolId);
                if (userRoles.Any())
                    return StatusCode(300,"Remove Users From Role First");
                var deleted = await roleCommandService.DeleteAsync(existingRole);

                return deleted ? Ok("Role deleted") : BadRequest("Error in deleting role");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }







    }
}
