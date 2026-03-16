using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.IdentityDTO;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.IdentityManagement.Command;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security;
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
        private readonly IUserRoleQueryService userRoleQueryService;
        private readonly IRolePermissionQueryService rolePermissionQueryService;
        private readonly INotificationFactory notificationFactory;
        private readonly IUserDeviceQueryService userDeviceQueryService;

        public UserPermissionController(IUserPermissionCommandService userPermissionCommandService,
                                        IUserPermissionQueryService userPermissionQueryService,
                                        ISchoolQueryService schoolQueryService,
                                        IPermissionQueryService permissionQueryService,
                                        IUserRoleQueryService userRoleQueryService,
                                        IRolePermissionQueryService rolePermissionQueryService,
                                           INotificationFactory notificationFactory,
                                           IUserDeviceQueryService userDeviceQueryService)
        {
            this.userPermissionCommandService = userPermissionCommandService;
            this.userPermissionQueryService = userPermissionQueryService;
            this.schoolQueryService = schoolQueryService;
            this.permissionQueryService = permissionQueryService;
            this.userRoleQueryService = userRoleQueryService;
            this.rolePermissionQueryService = rolePermissionQueryService;
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
                var permissionsToAdd = new List<UserPermission>();

                foreach (var group in dto.UserPermissions.GroupBy(p => new { p.UserId, p.SchoolId }))
                {
                    var existing = await userPermissionQueryService.GetUserPermissions(group.Key.SchoolId, group.Key.UserId);
                    var existingIds = existing != null
                        ? existing.Select(p => p.PermissionId).ToHashSet()
                        : new HashSet<Guid>();

                    var toAdd = group
                        .Where(u => !existingIds.Contains(u.PermissionId))
                        .Select(u => new UserPermission
                        {
                            UserId = u.UserId,
                            PermissionId = u.PermissionId,
                            SchoolId = u.SchoolId,
                            CreateDate = DateTime.Now,
                            LastModifyDate = DateTime.Now
                        });

                    permissionsToAdd.AddRange(toAdd);
                }

                if (permissionsToAdd.Count == 0)
                    return Ok("No permissions to add");

                var added = await userPermissionCommandService.Create(permissionsToAdd);

                if (added)
                {
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    foreach(var uPermissions in permissionsToAdd.GroupBy(p=> p.UserId).ToList())
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

        [HttpPost("AssignAllPermissionsToUser")]
        public async Task<IActionResult> AssignAllPermissionsToUser([FromBody] AssignAllPermissionsToUserDTO dto)
        {
            try
            {
                Guid? schoolId = dto.SchoolId;

                if (schoolId == null || schoolId == Guid.Empty)
                {
                    var userRoles = await userRoleQueryService.GetUserRolesDataAsync(dto.UserId, null);
                    var roleSchoolIds = userRoles.Where(r => r.SchoolId != null)
                                                 .Select(r => r.SchoolId!.Value)
                                                 .Distinct()
                                                 .ToList();

                    if (roleSchoolIds.Count == 1)
                        schoolId = roleSchoolIds[0];
                }

                if (schoolId == null || schoolId == Guid.Empty)
                    return BadRequest("SchoolId is required to assign permissions");

                var permissions = await permissionQueryService.GetAllAsync();
                var filteredPermissions = dto.IsSchoolUser
                    ? permissions.Where(p => p.SchoolPermission)
                    : permissions.Where(p => p.AppPermission);

                var userRolesData = await userRoleQueryService.GetUserRolesDataAsync(dto.UserId, dto.IsSchoolUser ? schoolId : null);
                var rolePermissionIds = new HashSet<Guid>();

                foreach (var role in userRolesData)
                {
                    var rolePermissions = await rolePermissionQueryService.GetRolePermissions(role.RoleId);
                    foreach (var permission in rolePermissions)
                        rolePermissionIds.Add(permission.PermissionId);
                }

                var existingPermissions = await userPermissionQueryService.GetUserPermissions(schoolId.Value, dto.UserId);
                var existingPermissionIds = existingPermissions != null
                    ? existingPermissions.Select(p => p.PermissionId).ToHashSet()
                    : new HashSet<Guid>();

                var permissionsToAssign = filteredPermissions
                    .Where(p => !rolePermissionIds.Contains(p.Id) && !existingPermissionIds.Contains(p.Id))
                    .Select(p => new UserPermission
                    {
                        UserId = dto.UserId,
                        PermissionId = p.Id,
                        SchoolId = schoolId.Value,
                        CreateDate = DateTime.Now,
                        LastModifyDate = DateTime.Now
                    })
                    .ToList();

                if (permissionsToAssign.Count == 0)
                    return Ok("No permissions to assign");

                var notAllowdPermissions = new List<Guid>();
                notAllowdPermissions.Add(Guid.Parse("9301FC37-AE75-4EF6-B6FA-A656452E5A2E"));
                notAllowdPermissions.Add(Guid.Parse("E9C02932-B613-45EB-9E71-7CB6204745D9"));

                permissionsToAssign = permissionsToAssign.Where(p => !notAllowdPermissions.Contains(p.PermissionId)).ToList();

                var added = await userPermissionCommandService.Create(permissionsToAssign);

                if (added)
                {
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var devices = await userDeviceQueryService.GetByUserIdAsync(dto.UserId);
                    var message = string.Empty;
                    if (dto.IsSchoolUser && dto.SchoolId.HasValue)
                    {
                        var school = await schoolQueryService.GetByIdAsync(dto.SchoolId.Value);
                        message = $"All permissions in school {school.Name} are assignd to you";
                    }
                    else
                    {
                        message = $"All permissions as application user are assignd to you";
                    }
                    
                    
                    if (devices.Count() > 0)
                    {
                        var tokens = devices.Select(d => d.FcmToken).ToList();
                        await notificationProvider.SendToMultiUsersAsync(tokens, "School Permission", message);
                    }

                    return Ok("Permissions added");
                }

                return BadRequest("Error in adding permissions");
            }
            catch (Exception ex)
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
                    if (userPermission != null)
                    {
                        removed = await userPermissionCommandService.DeleteAsync(userPermission);

                        notRemoved = string.IsNullOrEmpty(notRemoved) ? up.PermissionId.ToString() : $"{notRemoved}, {up.PermissionId.ToString()}";
                    }
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

        [HttpPost("RemoveAllUserPermissions")]
        public async Task<IActionResult> RemoveAllUserPermissions([FromBody] RemoveAllPermissionsFromUserDTO dto)
        {
            try
            {
                var userPermissions = dto.SchoolId.HasValue ?
                        await userPermissionQueryService.GetUserPermissions(dto.SchoolId.Value, dto.UserId) : 
                        await userPermissionQueryService.GetAppUserPermissions(dto.UserId);

                if (userPermissions == null || !userPermissions.Any())
                    return Ok("No permissions to remove");

                userPermissions = userPermissions.Where(p => p.PermissionId != Guid.Parse("9301FC37-AE75-4EF6-B6FA-A656452E5A2E")).ToList();

                var removed = await userPermissionCommandService.DeleteAsync(userPermissions);

                if (removed)
                {
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var devices = await userDeviceQueryService.GetByUserIdAsync(dto.UserId);
                    var message = string.Empty;
                    if (dto.SchoolId.HasValue)
                    {
                        var school = await schoolQueryService.GetByIdAsync(dto.SchoolId.Value);
                        message = $"All permissions in school {school.Name} are removed from you";
                    }
                    else
                    {
                        message = $"All permissions as application user are removed from you";
                    }


                    if (devices.Count() > 0)
                    {
                        var tokens = devices.Select(d => d.FcmToken).ToList();
                        await notificationProvider.SendToMultiUsersAsync(tokens, "School Permission", message);
                    }

                    return Ok("Permissions removed from user");
                }

                return BadRequest("Error in remove permissions from user");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



    }
}
