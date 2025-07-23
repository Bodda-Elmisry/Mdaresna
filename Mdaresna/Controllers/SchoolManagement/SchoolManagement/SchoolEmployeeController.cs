using Mdaresna.Doamin.DTOs.Identity;
using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO;
using Mdaresna.Infrastructure.Services.IdentityManagement.Query;
using Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Query;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.IdentityManagement.Command;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using static Dapper.SqlMapper;

namespace Mdaresna.Controllers.SchoolManagement.SchoolManagement
{
    [Route("SchoolEmployee")]
    public class SchoolEmployeeController : Controller
    {
        private readonly ISchoolEmployeeCommandService schoolEmployeeCommandService;
        private readonly ISchoolEmployeeQueryService schoolEmployeeQueryService;
        private readonly IUserRoleCommandService userRoleCommandService;
        private readonly IUserRoleQueryService userRoleQueryService;
        private readonly IUserPermissionCommandService userPermissionCommandService;
        private readonly IUserPermissionQueryService userPermissionQueryService;
        private readonly IRoleQueryService roleQueryService;
        private readonly IClassroomEmployeeCommandService classroomEmployeeCommandService;
        private readonly IClassroomEmployeeQueryService classroomEmployeeQueryService;
        private readonly ISchoolQueryService schoolQueryService;
        private readonly INotificationFactory notificationFactory;
        private readonly IUserDeviceQueryService userDeviceQueryService;

        public SchoolEmployeeController(ISchoolEmployeeCommandService schoolEmployeeCommandService,
                                        ISchoolEmployeeQueryService schoolEmployeeQueryService,
                                        IUserRoleCommandService userRoleCommandService,
                                        IUserRoleQueryService userRoleQueryService,
                                        IUserPermissionCommandService userPermissionCommandService,
                                        IUserPermissionQueryService userPermissionQueryService,
                                        IRoleQueryService roleQueryService,
                                        IClassroomEmployeeCommandService classroomEmployeeCommandService,
                                        IClassroomEmployeeQueryService classroomEmployeeQueryService,
                                        ISchoolQueryService schoolQueryService,
                                           INotificationFactory notificationFactory,
                                           IUserDeviceQueryService userDeviceQueryService)
        {
            this.schoolEmployeeCommandService = schoolEmployeeCommandService;
            this.schoolEmployeeQueryService = schoolEmployeeQueryService;
            this.userRoleCommandService = userRoleCommandService;
            this.userRoleQueryService = userRoleQueryService;
            this.userPermissionCommandService = userPermissionCommandService;
            this.userPermissionQueryService = userPermissionQueryService;
            this.roleQueryService = roleQueryService;
            this.classroomEmployeeCommandService = classroomEmployeeCommandService;
            this.classroomEmployeeQueryService = classroomEmployeeQueryService;
            this.schoolQueryService = schoolQueryService;
            this.notificationFactory = notificationFactory;
            this.userDeviceQueryService = userDeviceQueryService;
        }

        [HttpPost("AddSchoolEmployee")]
        public async Task<IActionResult> AddSchoolEmployee([FromBody] SchoolIdEmployeeIdDTO dto)
        {
            try
            {
                var teacher = await schoolEmployeeQueryService.GetSchoolEmployeeByIdAsync(dto.SchoolId, dto.EmployeeId);

                if (teacher != null)
                    return Conflict("Employee already assignd to school");

                var sEmployee = new SchoolEmployee
                {
                    SchoolId = dto.SchoolId,
                    EmployeeId = dto.EmployeeId
                };
                var added = schoolEmployeeCommandService.Create(sEmployee);
                if (added)
                {
                    //var addedRow = await schoolEmployeeQueryService.GetSchoolEmployeeByIdAsync(dto.SchoolId, dto.EmployeeId);

                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var devices = await userDeviceQueryService.GetByUserIdAsync(dto.EmployeeId);
                    var school = await schoolQueryService.GetByIdAsync(dto.SchoolId);
                    if (devices.Count() > 0)
                    {
                        var tokens = devices.Select(d => d.FcmToken).ToList();
                        await notificationProvider.SendToMultiUsersAsync(tokens, "School Role", $"Assigned as employee to new school {school.Name}|{school.Id}");
                    }

                    return Ok(dto);
                }
                return BadRequest("Can't add employee to school");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateSchoolEmployee")]
        public async Task<IActionResult> UpdateSchoolEmployee([FromBody] SchoolIdEmployeeIdDTO dto)
        {
            try
            {
                var sEmployee = await schoolEmployeeQueryService.GetSchoolEmployeeByIdAsync(dto.SchoolId, dto.EmployeeId);

                if (sEmployee == null)
                    return BadRequest("Can't update employee");



                var updated = schoolEmployeeCommandService.Update(sEmployee);

                if (updated)
                    return Ok(sEmployee);

                return BadRequest("Error in update school employee");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("RemoveEmployeeFromSchool")]
        public async Task<IActionResult> RemoveSchoolEmployee([FromBody] SchoolIdEmployeeIdDTO dto)
        {
            try
            {
                var sEmployee = new SchoolEmployee
                {
                    SchoolId = dto.SchoolId,
                    EmployeeId = dto.EmployeeId
                };
                var employeeRoles = await userRoleQueryService.GetUserRolesDataAsync(dto.EmployeeId, dto.SchoolId);
                var employeePermissions = await userPermissionQueryService.GetUserPermissions(dto.SchoolId, dto.EmployeeId);
                var deleted = await schoolEmployeeCommandService.DeleteAsync(sEmployee);
                if (deleted)
                {
                    if (employeeRoles != null && employeeRoles.Count() > 0)
                    {

                        foreach (var role in employeeRoles)
                        {
                            await userRoleCommandService.DeleteAsync(role);
                        }
                    }

                    if (employeePermissions != null && employeePermissions.Count() > 0)
                    {
                        foreach (var permission in employeePermissions)
                        {
                            await userPermissionCommandService.DeleteAsync(permission);
                        }

                    }

                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var devices = await userDeviceQueryService.GetByUserIdAsync(dto.EmployeeId);
                    var school = await schoolQueryService.GetByIdAsync(dto.SchoolId);
                    if (devices.Count() > 0)
                    {
                        var tokens = devices.Select(d => d.FcmToken).ToList();
                        await notificationProvider.SendToMultiUsersAsync(tokens, "School Role", $"removed as employee from school {school.Name}|{school.Id}");
                    }
                    return Ok("Employee removed from school");
                }

                return BadRequest("Error in removing");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetSchoolEmployees")]
        public async Task<IActionResult> GetSchoolEmployees([FromBody] GetSchoolEmployeesDTO dto)
        {
            try
            {

                var employees = await schoolEmployeeQueryService.GetSchoolEmployeesAsync(dto.SchoolId, dto.EmployeeName, dto.EmployeePhoneNumber, dto.EmployeeEmail);
                return Ok(employees);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetEmployeeSchools")]
        public async Task<IActionResult> GetEmployeeSchools([FromBody] EmployeeIdDTO employeeIdDTO)
        {
            try
            {
                var schools = await schoolEmployeeQueryService.GetEmployeeSchoolsAsync(employeeIdDTO.EmployeeId);
                return Ok(schools);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteSchoolEmployee")]
        public async Task<IActionResult> SoftDeleteSchoolEmployee([FromBody] SchoolIdEmployeeIdDTO dto)
        {
            try
            {

                var employeeClassrooms = await classroomEmployeeQueryService.GetEmployeeClassroomsAsync(dto.EmployeeId, dto.SchoolId);

                var sEmployee = await schoolEmployeeQueryService.GetSchoolEmployeeByIdAsync(dto.SchoolId, dto.EmployeeId);
                if (sEmployee == null)
                    return BadRequest("Can't find employee to delete");

                if (employeeClassrooms != null && employeeClassrooms.Count() > 0)
                {

                    foreach (var classroom in employeeClassrooms)
                    {


                        await classroomEmployeeCommandService.DeleteAsync(classroom);
                    }
                }

                var deleted = await schoolEmployeeCommandService.DeleteAsync(sEmployee);
                if (deleted)
                    return Ok("Employee removed from school");

                return BadRequest("Error in removing Employee");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }







        }
    }
}
