using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.Identity;
using Mdaresna.DTOs.IdentityDTO;
using Mdaresna.Infrastructure.Services.IdentityManagement.Command;
using Mdaresna.Infrastructure.Services.IdentityManagement.Query;
using Mdaresna.Infrastructure.Services.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.IdentityManagement.Command;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Microsoft.AspNetCore.Mvc;
using System.Security;

namespace Mdaresna.Controllers.IdentityManagement
{
    [Route("UserPermissionSchoolClassroom")]
    public class UserPermissionSchoolClassRoomController : Controller
    {
        private readonly IUserPermissionSchoolClassRoomCommandService userPermissionSchoolClassRoomCommandService;
        private readonly IUserPermissionSchoolClassRoomQueryService userPermissionSchoolClassRoomQueryService;
        private readonly IPermissionQueryService permissionQueryService;
        private readonly IClassRoomQueryService classroomQueryService;
        private readonly ISchoolQueryService schoolQueryService;
        private readonly INotificationFactory notificationFactory;
        private readonly IUserDeviceQueryService userDeviceQueryService;

        public UserPermissionSchoolClassRoomController(IUserPermissionSchoolClassRoomCommandService userPermissionSchoolClassRoomCommandService,
                                                       IUserPermissionSchoolClassRoomQueryService userPermissionSchoolClassRoomQueryService,
                                        IPermissionQueryService permissionQueryService,
                                                IClassRoomQueryService classroomQueryService,
                                                ISchoolQueryService schoolQueryService,
                                           INotificationFactory notificationFactory,
                                           IUserDeviceQueryService userDeviceQueryService)
        {
            this.userPermissionSchoolClassRoomCommandService = userPermissionSchoolClassRoomCommandService;
            this.userPermissionSchoolClassRoomQueryService = userPermissionSchoolClassRoomQueryService;
            this.permissionQueryService = permissionQueryService;
            this.classroomQueryService = classroomQueryService;
            this.schoolQueryService = schoolQueryService;
            this.notificationFactory = notificationFactory;
            this.userDeviceQueryService = userDeviceQueryService;
        }

        [HttpPost("AssignUserPermissionSchoolClassrooms")]
        public async Task<IActionResult> AssignUserPermissionSchoolClassrooms([FromBody] AssignAndRemoveUserPermissionSchoolClassroomDTO dto)
        {
            try
            {
                var userPermissionsClassrooms = dto.UserPermissionsClassrooms.Select(u => new UserPermissionSchoolClassRoom
                {
                    UserId = u.UserId,
                    PermissionId = u.PermissionId,
                    ClassRoomId = u.ClassRoomId,
                    CreateDate = DateTime.Now,
                    LastModifyDate = DateTime.Now
                });

                var added = await userPermissionSchoolClassRoomCommandService.Create(userPermissionsClassrooms);

                if (added)
                {
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var devices = await userDeviceQueryService.GetByUserIdAsync(dto.UserPermissionsClassrooms.FirstOrDefault().UserId);
                    var classroom = await classroomQueryService.GetByIdAsync(dto.UserPermissionsClassrooms.FirstOrDefault().ClassRoomId);
                    var school = await schoolQueryService.GetByIdAsync(classroom.SchoolId);
                    var message = $"Assigend new permissions in school {school.Name}|{school.Id}";
                    if (devices.Count() > 0)
                    {
                        var tokens = devices.Select(d => d.FcmToken).ToList();
                        await notificationProvider.SendToMultiUsersAsync(tokens, "School Permission", message);
                    }
                }

                return added ? Ok("Clasrooms added") : BadRequest("Error in adding classrooms");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("RemoveUserPermissionSchoolClassrooms")]
        public async Task<IActionResult> RemovePermissionsFromUser([FromBody] AssignAndRemoveUserPermissionSchoolClassroomDTO dto)
        {
            try
            {
                var notRemoved = string.Empty;
                var removed = true;

                foreach (var up in dto.UserPermissionsClassrooms)
                {
                    var userPermission = await userPermissionSchoolClassRoomQueryService.GetUserPermissionSchoolClassRoomByIdAsync(up.UserId, up.PermissionId, up.ClassRoomId);

                    removed = await userPermissionSchoolClassRoomCommandService.DeleteAsync(userPermission);

                    notRemoved = string.IsNullOrEmpty(notRemoved) ? up.PermissionId.ToString() : $", {up.PermissionId.ToString()}";
                }

                if (!string.IsNullOrEmpty(notRemoved))
                {
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var devices = await userDeviceQueryService.GetByUserIdAsync(dto.UserPermissionsClassrooms.FirstOrDefault().UserId);
                    var classroom = await classroomQueryService.GetByIdAsync(dto.UserPermissionsClassrooms.FirstOrDefault().ClassRoomId);
                    var school = await schoolQueryService.GetByIdAsync(classroom.SchoolId);
                    var message = $"Permissions removed from school {school.Name}|{school.Id}";
                    if (devices.Count() > 0)
                    {
                        var tokens = devices.Select(d => d.FcmToken).ToList();
                        await notificationProvider.SendToMultiUsersAsync(tokens, "School Permission", message);
                    }
                }

                return string.IsNullOrEmpty(notRemoved) ? Ok("Classroom removed from permissions") : BadRequest("Error in remove some classrooms from permissions");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }





    }
}
