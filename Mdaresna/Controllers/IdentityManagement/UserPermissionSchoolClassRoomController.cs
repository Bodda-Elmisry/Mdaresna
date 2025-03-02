using Mdaresna.Doamin.Models.Identity;
using Mdaresna.DTOs.IdentityDTO;
using Mdaresna.Infrastructure.Services.IdentityManagement.Command;
using Mdaresna.Infrastructure.Services.IdentityManagement.Query;
using Mdaresna.Repository.IServices.IdentityManagement.Command;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.IdentityManagement
{
    [Route("UserPermissionSchoolClassroom")]
    public class UserPermissionSchoolClassRoomController : Controller
    {
        private readonly IUserPermissionSchoolClassRoomCommandService userPermissionSchoolClassRoomCommandService;
        private readonly IUserPermissionSchoolClassRoomQueryService userPermissionSchoolClassRoomQueryService;

        public UserPermissionSchoolClassRoomController(IUserPermissionSchoolClassRoomCommandService userPermissionSchoolClassRoomCommandService,
                                                       IUserPermissionSchoolClassRoomQueryService userPermissionSchoolClassRoomQueryService)
        {
            this.userPermissionSchoolClassRoomCommandService = userPermissionSchoolClassRoomCommandService;
            this.userPermissionSchoolClassRoomQueryService = userPermissionSchoolClassRoomQueryService;
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

                return string.IsNullOrEmpty(notRemoved) ? Ok("Classroom removed from permissions") : BadRequest("Error in remove some classrooms from permissions");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }





    }
}
