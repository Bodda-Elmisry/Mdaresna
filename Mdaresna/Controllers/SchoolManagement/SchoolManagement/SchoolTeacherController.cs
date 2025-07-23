using Mdaresna.Doamin.DTOs.Identity;
using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.Infrastructure.Services.IdentityManagement.Query;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.IdentityManagement.Command;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.SchoolManagement
{
    [Route("SchoolTeacher")]
    public class SchoolTeacherController : Controller
    {
        private readonly ISchoolTeacherCommandService schoolTeacherCommandService;
        private readonly ISchoolTeacherQueryService schoolTeacherQueryService;
        private readonly IUserRoleCommandService userRoleCommandService;
        private readonly IUserRoleQueryService userRoleQueryService;
        private readonly IUserPermissionQueryService userPermissionQueryService;
        private readonly IUserPermissionCommandService userPermissionCommandService;
        private readonly IClassRoomTeacherCourseQueryService classRoomTeacherCourseQueryService;
        private readonly IClassRoomTeacherCourseCommandService classRoomTeacherCourseCommandService;
        private readonly ISchoolQueryService schoolQueryService;
        private readonly INotificationFactory notificationFactory;
        private readonly IUserDeviceQueryService userDeviceQueryService;

        public SchoolTeacherController(ISchoolTeacherCommandService schoolTeacherCommandService,
                                       ISchoolTeacherQueryService schoolTeacherQueryService,
                                       IUserRoleCommandService userRoleCommandService,
                                       IUserRoleQueryService userRoleQueryService,
                                       IUserPermissionQueryService userPermissionQueryService,
                                       IUserPermissionCommandService userPermissionCommandService,
                                       IClassRoomTeacherCourseQueryService classRoomTeacherCourseQueryService,
                                       IClassRoomTeacherCourseCommandService classRoomTeacherCourseCommandService,
                                       ISchoolQueryService schoolQueryService,
                                           INotificationFactory notificationFactory,
                                           IUserDeviceQueryService userDeviceQueryService)
        {
            this.schoolTeacherCommandService = schoolTeacherCommandService;
            this.schoolTeacherQueryService = schoolTeacherQueryService;
            this.userRoleCommandService = userRoleCommandService;
            this.userRoleQueryService = userRoleQueryService;
            this.userPermissionQueryService = userPermissionQueryService;
            this.userPermissionCommandService = userPermissionCommandService;
            this.classRoomTeacherCourseQueryService = classRoomTeacherCourseQueryService;
            this.classRoomTeacherCourseCommandService = classRoomTeacherCourseCommandService;
            this.schoolQueryService = schoolQueryService;
            this.notificationFactory = notificationFactory;
            this.userDeviceQueryService = userDeviceQueryService;
        }

        [HttpPost("AddSchoolTeacher")]
        public async Task<IActionResult> AddSchoolTeacher([FromBody] SchoolIdTeacherIdDTO schoolTeacher)
        {
            try
            {
                var teacher = await schoolTeacherQueryService.GetSchoolTeacherByIdAsync(schoolTeacher.SchoolId, schoolTeacher.TeacherId);

                if (teacher != null)
                    return Conflict("Teacher already assignd to school");
                
                var sTeacher = new SchoolTeacher
                {
                    SchoolId = schoolTeacher.SchoolId,
                    TeacherId = schoolTeacher.TeacherId
                };
                var added = schoolTeacherCommandService.Create(sTeacher);
                if (added)
                {
                    var userrole = new UserRole
                    {
                        RoleId = Guid.Parse("10620C5F-37FE-4D18-996F-915ECE8893F1"),
                        UserId = sTeacher.TeacherId,
                        SchoolId = schoolTeacher.SchoolId
                    };
                    var teacherRoleAssignd = await userRoleQueryService.CheckRoleExist(userrole);
                    if (!teacherRoleAssignd)
                    {
                        var roleadded = userRoleCommandService.Create(userrole);
                        if (!roleadded)
                            return BadRequest("Error in adding teacher role");
                    }

                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var devices = await userDeviceQueryService.GetByUserIdAsync(schoolTeacher.TeacherId);
                    var school = await schoolQueryService.GetByIdAsync(schoolTeacher.SchoolId);
                    if (devices.Count() > 0)
                    {
                        var tokens = devices.Select(d => d.FcmToken).ToList();
                        await notificationProvider.SendToMultiUsersAsync(tokens, "School Role", $"Assigned as a teacher to school {school.Name}|{school.Id}");
                    }
                    return Ok(schoolTeacher);
                }
                return BadRequest("Can't add teacher to school");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateSchoolTeacher")]
        public async Task<IActionResult> UpdateSchoolTeacher([FromBody] SchoolIdTeacherIdDTO schoolTeacher)
        {
            try
            {
                var sTeacher = await schoolTeacherQueryService.GetSchoolTeacherByIdAsync(schoolTeacher.SchoolId, schoolTeacher.TeacherId);

                if (sTeacher == null)
                    return BadRequest("Can't update teacher");

                

                var updated = schoolTeacherCommandService.Update(sTeacher);

                if(updated)
                    return Ok(sTeacher);

                return BadRequest("Error in update school teacher");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("RemoveTeacherFromSchool")]
        public async Task<IActionResult> RemoveSchoolTeacher([FromBody] SchoolIdTeacherIdDTO schoolTeacher)
        {
            try
            {
                var sTeacher = new SchoolTeacher
                {
                    SchoolId = schoolTeacher.SchoolId,
                    TeacherId = schoolTeacher.TeacherId
                };
                var techerRoles = await userRoleQueryService.GetUserRolesDataAsync(schoolTeacher.TeacherId, schoolTeacher.SchoolId);
                var teacherPermissions = await userPermissionQueryService.GetUserPermissions(schoolTeacher.SchoolId, schoolTeacher.TeacherId);
                var deleted = await schoolTeacherCommandService.DeleteAsync(sTeacher);
                if (deleted)
                {
                    if (techerRoles != null && techerRoles.Count() > 0)
                    {
                        
                        foreach (var role in techerRoles)
                        {
                            await userRoleCommandService.DeleteAsync(role);
                        }
                    }

                    if(teacherPermissions != null && teacherPermissions.Count() > 0)
                    {
                        foreach (var permission in teacherPermissions)
                        {
                            await userPermissionCommandService.DeleteAsync(permission);
                        }
                        
                    }

                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var devices = await userDeviceQueryService.GetByUserIdAsync(schoolTeacher.TeacherId);
                    var school = await schoolQueryService.GetByIdAsync(schoolTeacher.SchoolId);
                    if (devices.Count() > 0)
                    {
                        var tokens = devices.Select(d => d.FcmToken).ToList();
                        await notificationProvider.SendToMultiUsersAsync(tokens, "School Role", $"removed as a teacher from school {school.Name}|{school.Id}");
                    }
                    return Ok("Teacher removed from school");
                }

                return BadRequest("Error in removing");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetSchoolTeachers")]
        public async Task<IActionResult> GetSchoolTeachers([FromBody] GetSchoolTeachersDTO dto)
        {
            try
            {
                
                var teachers = await schoolTeacherQueryService.GetSchoolTeachersAsync(dto.SchoolId, dto.TeacherName, dto.TeacherPhoneNumber, dto.TeacherEmail);
                return Ok(teachers);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetTeacherSchools")]
        public async Task<IActionResult> GetTeacherSchools([FromBody] TeacherIdDTO teacherIdDTO)
        {
            try
            {
                var schools = await schoolTeacherQueryService.GetTeacherSchoolsAsync(teacherIdDTO.TeacherId);
                return Ok(schools);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteSchoolTeacher")]
        public async Task<IActionResult> SoftDeleteSchoolTeacher([FromBody] SchoolIdTeacherIdDTO dto)
        {
            try
            {

                var teacherClassrooms = await classRoomTeacherCourseQueryService.GetTeacherClassroomsCoursesAsync(dto.TeacherId, dto.SchoolId);

                var sTeacher = await schoolTeacherQueryService.GetSchoolTeacherByIdAsync(dto.SchoolId, dto.TeacherId);
                if (sTeacher == null)
                    return BadRequest("Can't find teacher to delete");

                if (teacherClassrooms != null && teacherClassrooms.Count() > 0)
                {

                    foreach (var classroom in teacherClassrooms)
                    {


                        await classRoomTeacherCourseCommandService.DeleteAsync(classroom);
                    }
                }

                var deleted = await schoolTeacherCommandService.DeleteAsync(sTeacher);
                if (deleted)
                    return Ok("Teacher removed from school");

                return BadRequest("Error in removing Teacher");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }







        }











    }
}
