using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.IdentityDTO;
using Mdaresna.Infrastructure.Services.IdentityManagement.Query;
using Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.IdentityManagement.Command;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Mdaresna.Controllers.IdentityManagement
{
    [Route("UserRole")]
    public class UserRoleController : Controller
    {
        private readonly IUserRoleQueryService userRoleQueryService;
        private readonly IUserRoleCommandService userRoleCommandService;
        private readonly IRoleQueryService roleQueryService;
        private readonly ISchoolQueryService schoolQueryService;
        private readonly INotificationFactory notificationFactory;
        private readonly IUserDeviceQueryService userDeviceQueryService;

        public UserRoleController(IUserRoleQueryService userRoleQueryService,
                                  IUserRoleCommandService userRoleCommandService,
                                  IRoleQueryService roleQueryService,
                                  ISchoolQueryService schoolQueryService,
                                  INotificationFactory notificationFactory,
                                  IUserDeviceQueryService userDeviceQueryService)
        {
            this.userRoleQueryService = userRoleQueryService;
            this.userRoleCommandService = userRoleCommandService;
            this.roleQueryService = roleQueryService;
            this.schoolQueryService = schoolQueryService;
            this.notificationFactory = notificationFactory;
            this.userDeviceQueryService = userDeviceQueryService;
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

        [HttpGet("GetSchoolsAdmins")]
        public async Task<IActionResult> GetSchoolsAdmins()
        {
            try
            {
                var userRoles = await userRoleQueryService.GetSchoolsAdminsAsync();
                return Ok(userRoles);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetApplicationManagers")]
        public async Task<IActionResult> GetApplicationManagers([FromBody] GetApplicationManagersDTO dto)
        {
            try
            {
                var userRoles = await userRoleQueryService.GetApplicationManagersAsync(dto.Name, dto.PhoneNumber);
                return Ok(userRoles);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetRoleUsers")]
        public async Task<IActionResult> GetRoleUsers([FromBody] GetRoleUsersDTO dTO)
        {
            try
            {
                var userRoles = await userRoleQueryService.GetRoleUsersAsync(dTO.RoleId, dTO.SchoolId);
                return Ok(userRoles);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetUserRole")]
        public async Task<IActionResult> GetUserRole([FromBody] GetUserRoleDTO dTO)
        {
            try
            {
                var userRole = await userRoleQueryService.GetUserRoleViewAsync(dTO.UserId, dTO.RoleId, dTO.SchoolId);
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
                if (added)
                {
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var message = string.Empty;
                    foreach (var uRoles in dTO.UserRoles.GroupBy(p => p.UserId).ToList())
                    {
                        StringBuilder rolesNames = new StringBuilder();
                        foreach (var role in uRoles)
                        {
                            if (rolesNames.Length > 0)
                                rolesNames.Append(", ");
                            var roleInfo = await roleQueryService.GetByIdAsync(role.RoleId);
                            rolesNames.Append(roleInfo.Name);
                        }
                        var devices = await userDeviceQueryService.GetByUserIdAsync(uRoles.FirstOrDefault().UserId);
                        if (uRoles.FirstOrDefault().SchoolId != null)
                        {

                            var school = await schoolQueryService.GetByIdAsync(uRoles.FirstOrDefault().SchoolId.Value);
                            message = $"Assigend new role {rolesNames.ToString()} in school {school.Name}|{school.Id}";
                        }
                        else
                        {
                            message = $"Assigend new role {rolesNames.ToString()}";

                        }
                        if (devices.Count() > 0)
                        {
                            var tokens = devices.Select(d => d.FcmToken).ToList();
                            await notificationProvider.SendToMultiUsersAsync(tokens, "Role", message);
                        }
                    }
                        return Ok("Roles assignd to user");
                }

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
                    //var userRole = new UserRole
                    //{
                    //    UserId = ur.UserId,
                    //    RoleId = ur.RoleId,
                    //    SchoolId = ur.SchoolId
                    //};

                    var userRole = await userRoleQueryService.GetUserRoleAsync(ur.UserId, ur.RoleId, ur.SchoolId);


                    if (userRole != null)
                    {   
                        removed = await userRoleCommandService.DeleteAsync(userRole);
                    }
                    if(!removed)
                        unRemoved = string.IsNullOrEmpty(unRemoved) ? unRemoved : $"{unRemoved}, {ur.RoleId.ToString()}";
                }

                if (string.IsNullOrEmpty(unRemoved))
                {
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var message = string.Empty;
                    foreach (var uRoles in dTO.UserRoles.GroupBy(p => p.UserId).ToList())
                    {
                        StringBuilder rolesNames = new StringBuilder();
                        foreach (var role in uRoles)
                        {
                            if (rolesNames.Length > 0)
                                rolesNames.Append(", ");
                            var roleInfo = await roleQueryService.GetByIdAsync(role.RoleId);
                            rolesNames.Append(roleInfo.Name);
                        }
                        var devices = await userDeviceQueryService.GetByUserIdAsync(uRoles.FirstOrDefault().UserId);
                        if (uRoles.FirstOrDefault().SchoolId != null)
                        {

                            var school = await schoolQueryService.GetByIdAsync(uRoles.FirstOrDefault().SchoolId.Value);
                            message = $"Removed role {rolesNames.ToString()} from school {school.Name}|{school.Id}";
                        }
                        else
                        {
                            message = $"Removed role {rolesNames.ToString()}";

                        }
                        if (devices.Count() > 0)
                        {
                            var tokens = devices.Select(d => d.FcmToken).ToList();
                            await notificationProvider.SendToMultiUsersAsync(tokens, "Role", message);
                        }
                    }

                    return Ok("Roles removed from user");
                }

                return BadRequest("Error in remove roles from user ");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



    }
}
