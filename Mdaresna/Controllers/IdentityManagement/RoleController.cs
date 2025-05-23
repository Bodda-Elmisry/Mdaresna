﻿using Mdaresna.Doamin.Models.Identity;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.IdentityDTO;
using Mdaresna.Repository.IServices.IdentityManagement.Command;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.IdentityManagement
{
    [Route("Role")]
    public class RoleController : Controller
    {
        private readonly IRoleCommandService roleCommandService;
        private readonly IRoleQueryService roleQueryService;
        private readonly IUserRoleQueryService userRoleQueryService;

        public RoleController(IRoleCommandService roleCommandService,
                              IRoleQueryService roleQueryService, 
                              IUserRoleQueryService userRoleQueryService)
        {
            this.roleCommandService = roleCommandService;
            this.roleQueryService = roleQueryService;
            this.userRoleQueryService = userRoleQueryService;
        }

        [HttpPost("GetRolesList")]
        public async Task<IActionResult> GetRolesList([FromBody] GetRolesDTO dTO)
        {
            try
            {
                var ignoredRoles = new List<Guid>();
                ignoredRoles.Add(Guid.Parse("92D00B28-9D25-4BD2-A587-6C22A3A07A92")); //Standerd
                ignoredRoles.Add(Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73")); //school manager
                ignoredRoles.Add(Guid.Parse("10620C5F-37FE-4D18-996F-915ECE8893F1")); //school teacher
                var roles = await roleQueryService.GetRolesAsync(dTO.Type, dTO.Name, dTO.Activation, dTO.Description, ignoredRoles);

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
                var existingRolesWithTheSameName = await roleQueryService.GetRolesAsync(
                    dTO.IsSchoolRole ? 1 : 2,
                    dTO.Name,
                    true,
                    null
                    );

                if (existingRolesWithTheSameName != null && existingRolesWithTheSameName.Count() > 0)
                    return Conflict("Role already exist");

                var role = new Role
                {
                    Name = dTO.Name,
                    Description = dTO.Description,
                    Active = dTO.Active,
                    AdminRole = !dTO.IsSchoolRole,
                    SchoolRole = dTO.IsSchoolRole
                };

                var added = await roleCommandService.Create(role, dTO.Permissions);

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
                var userRoles = await userRoleQueryService.GetRoleUsersAsync(dto.RoleId, null);
                if (userRoles != null)
                    return BadRequest("Remove Users From Role First");
                var role = new Role
                {
                    Id = dto.RoleId,
                };
                var deleted = await roleCommandService.DeleteAsync(role);

                return deleted ? Ok("Role deleted") : BadRequest("Error in deleting role");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }







    }
}
