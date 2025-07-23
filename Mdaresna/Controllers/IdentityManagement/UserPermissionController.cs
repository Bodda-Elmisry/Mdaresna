using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.Identity;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.IdentityDTO;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.IdentityManagement.Command;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Mdaresna.Controllers.IdentityManagement
{
    [Route("UserPermission")]
    public class UserPermissionController : Controller
    {
        private readonly IUserPermissionCommandService userPermissionCommandService;
        private readonly IUserPermissionQueryService userPermissionQueryService;
        private readonly ISchoolQueryService schoolQueryService;
        private readonly IPermissionQueryService permissionQueryService;
        private readonly INotificationFactory notificationFactory;
        private readonly IUserDeviceQueryService userDeviceQueryService;

        public UserPermissionController(IUserPermissionCommandService userPermissionCommandService,
                                        IUserPermissionQueryService userPermissionQueryService,
                                        ISchoolQueryService schoolQueryService,
                                        IPermissionQueryService permissionQueryService,
                                           INotificationFactory notificationFactory,
                                           IUserDeviceQueryService userDeviceQueryService)
        {
            this.userPermissionCommandService = userPermissionCommandService;
            this.userPermissionQueryService = userPermissionQueryService;
            this.schoolQueryService = schoolQueryService;
            this.permissionQueryService = permissionQueryService;
            this.notificationFactory = notificationFactory;
            this.userDeviceQueryService = userDeviceQueryService;
        }

        [HttpPost("GetUserPermissions")]
        public async Task<IActionResult> GetUserPermissions([FromBody] GetUserPermissionsDTO dTO)
        {
            try
            {
                var permissions = await userPermissionQueryService.GetUserPermissionsView(dTO.UserId, dTO.SchoolId);

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

                if (added)
                {
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    foreach(var uPermissions in userPermissions.GroupBy(p=> p.UserId).ToList())
                    {
                        StringBuilder permissionsNames = new StringBuilder();
                        foreach (var permission in uPermissions) 
                        {
                            if(permissionsNames.Length > 0)
                                permissionsNames.Append(", ");
                            var permissionInfo = await permissionQueryService.GetByIdAsync(permission.PermissionId);
                            permissionsNames.Append(permissionInfo.Name);
                        }
                        var devices = await userDeviceQueryService.GetByUserIdAsync(uPermissions.FirstOrDefault().UserId);
                        var school = await schoolQueryService.GetByIdAsync(uPermissions.FirstOrDefault().SchoolId);
                        var message = $"Assigend new permissions {permissionsNames.ToString()} in school {school.Name}|{school.Id}";
                        if (devices.Count() > 0)
                        {
                            var tokens = devices.Select(d => d.FcmToken).ToList();
                            await notificationProvider.SendToMultiUsersAsync(tokens, "School Permission", message);
                        }
                    }
                    
                    return Ok("Permissions added");
                }

                return BadRequest("Error in adding permissions");
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

                if (!string.IsNullOrEmpty(notRemoved))
                {
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    foreach (var uPermissions in dto.UserPermissions.GroupBy(p => p.UserId).ToList())
                    {
                        StringBuilder permissionsNames = new StringBuilder();
                        foreach (var permission in uPermissions)
                        {
                            if (permissionsNames.Length > 0)
                                permissionsNames.Append(", ");
                            
                            var permissionInfo = await permissionQueryService.GetByIdAsync(permission.PermissionId);

                            permissionsNames.Append(permissionInfo.Name);
                        }
                        var devices = await userDeviceQueryService.GetByUserIdAsync(uPermissions.FirstOrDefault().UserId);
                        var school = await schoolQueryService.GetByIdAsync(uPermissions.FirstOrDefault().SchoolId);
                        var message = $"Removed permissions {permissionsNames.ToString()} from school {school.Name}|{school.Id}";
                        if (devices.Count() > 0)
                        {
                            var tokens = devices.Select(d => d.FcmToken).ToList();
                            await notificationProvider.SendToMultiUsersAsync(tokens, "School Permission", message);
                        }
                    }

                    return Ok("Permissions removed from user");
                }

                return BadRequest("Error in remove some permissions from user");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



    }
}
