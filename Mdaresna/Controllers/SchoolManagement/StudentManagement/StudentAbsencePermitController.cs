using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Enums;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mdaresna.Controllers.SchoolManagement.StudentManagement
{
    [Authorize]
    [Route("AbsencePermit")]
    public class StudentAbsencePermitController : Controller
    {
        private readonly IStudentAbsencePermitCommandService studentAbsencePermitCommandService;
        private readonly IStudentAbsencePermitQueryService studentAbsencePermitQueryService;
        private readonly IStudentQueryService studentQueryService;
        private readonly IUserPermissionQueryService userPermissionQueryService;
        private readonly IUserDeviceQueryService userDeviceQueryService;
        private readonly INotificationFactory notificationFactory;

        public StudentAbsencePermitController(
            IStudentAbsencePermitCommandService studentAbsencePermitCommandService,
            IStudentAbsencePermitQueryService studentAbsencePermitQueryService,
            IStudentQueryService studentQueryService,
            IUserPermissionQueryService userPermissionQueryService,
            IUserDeviceQueryService userDeviceQueryService,
            INotificationFactory notificationFactory)
        {
            this.studentAbsencePermitCommandService = studentAbsencePermitCommandService;
            this.studentAbsencePermitQueryService = studentAbsencePermitQueryService;
            this.studentQueryService = studentQueryService;
            this.userPermissionQueryService = userPermissionQueryService;
            this.userDeviceQueryService = userDeviceQueryService;
            this.notificationFactory = notificationFactory;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] AddStudentAbsencePermitDTO permitDTO)
        {
            try
            {
                if (permitDTO.StudentId == Guid.Empty || permitDTO.ParentId == Guid.Empty)
                {
                    return BadRequest("Student and parent are required");
                }

                var result = await studentAbsencePermitCommandService.CreateAbsencePermitAsync(permitDTO);

                if (result == "Absence Permit Created")
                {
                    try
                    {
                        var student = await studentQueryService.GetStudentByIdAsync(permitDTO.StudentId);
                        if (student != null)
                        {
                            var userIds = await userPermissionQueryService.GetPermissionUsersIdsByPermissionKey("ApproveAbsencePermit", student.SchoolId);
                            if (userIds != null && userIds.Any())
                            {
                                var devices = await userDeviceQueryService.GetUsersDevicesAsync(userIds);
                                if (devices != null && devices.Any())
                                {
                                    var tokens = devices
                                        .Select(d => d.FcmToken)
                                        .Where(t => !string.IsNullOrWhiteSpace(t))
                                        .Distinct()
                                        .ToList();

                                    if (tokens.Any())
                                    {
                                        var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                                        var studentName = $"{student.FirstName} {student.LastName}";
                                        var message = $"تم تقديم طلب عذر غياب جديد للطالب {studentName} وبانتظار المراجعة.|Type=AbsencePermit|StudentId={permitDTO.StudentId}|SchoolId={student.SchoolId}";
                                        await notificationProvider.SendToMultiUsersAsync(tokens, "طلب عذر غياب جديد", message);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore notification errors to avoid failing the main request
                    }
                }

                return result == "Absence Permit Created"
                    ? Ok(result)
                    : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetStudentPermits")]
        public async Task<IActionResult> GetStudentPermits([FromBody] GetStudentAbsencePermitsDTO permitsDTO)
        {
            try
            {
                if (permitsDTO.StudentId == null && permitsDTO.ParentId == null)
                {
                    return BadRequest("Can't get data without student or parent");
                }

                var data = await studentAbsencePermitQueryService.GetStudentAbsencePermitsAsync(
                    permitsDTO.StudentId,
                    permitsDTO.ParentId,
                    permitsDTO.PageNumber);

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDelete")]
        public async Task<IActionResult> SoftDelete([FromBody] StudentAbsencePermitIdDTO dto)
        {
            try
            {
                var deleted = await studentAbsencePermitCommandService.SoftDeleteAbsencePermitAsync(
                    dto.StudentAbsencePermitId,
                    dto.ParentId);

                return deleted
                    ? Ok("Absence Permit Deleted")
                    : BadRequest("Error in deleting absence permit");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Review")]
        [Mdaresna.Middlewares.PermissionAuthorize("ApproveAbsencePermit")]
        public async Task<IActionResult> Review([FromBody] ReviewStudentAbsencePermitDTO dto)
        {
            try
            {
                var result = await studentAbsencePermitCommandService.ReviewAbsencePermitAsync(dto);

                if (result == "Absence Permit Reviewed")
                {
                    try
                    {
                        var permit = await studentAbsencePermitQueryService.GetByIdAsync(dto.StudentAbsencePermitId);
                        if (permit != null)
                        {
                            var student = await studentQueryService.GetStudentByIdAsync(permit.StudentId);
                            if (student != null)
                            {
                                var devices = await userDeviceQueryService.GetByUserIdAsync(permit.ParentId);
                                if (devices != null && devices.Any())
                                {
                                    var tokens = devices
                                        .Select(d => d.FcmToken)
                                        .Where(t => !string.IsNullOrWhiteSpace(t))
                                        .Distinct()
                                        .ToList();

                                    if (tokens.Any())
                                    {
                                        var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                                        var studentName = $"{student.FirstName} {student.LastName}";
                                        
                                        string statusAr = dto.Status == AbsencePermitStatusEnum.Approved ? "الموافقة على" : "رفض";
                                        
                                        var message = $"تم {statusAr} عذر الغياب المقدم للطالب {studentName} ليوم {permit.Date:yyyy/MM/dd}.|Type=AbsencePermit|StudentId={permit.StudentId}|SchoolId={student.SchoolId}";
                                        await notificationProvider.SendToMultiUsersAsync(tokens, "تحديث عذر الغياب", message);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore notification errors
                    }
                }

                return result == "Absence Permit Reviewed"
                    ? Ok(result)
                    : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
