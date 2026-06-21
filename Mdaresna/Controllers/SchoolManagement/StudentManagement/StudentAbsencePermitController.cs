using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Enums;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


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
        private readonly ISchoolQueryService schoolQueryService;
        private readonly ISchoolAccessValidator schoolAccessValidator;

        public StudentAbsencePermitController(
            IStudentAbsencePermitCommandService studentAbsencePermitCommandService,
            IStudentAbsencePermitQueryService studentAbsencePermitQueryService,
            IStudentQueryService studentQueryService,
            IUserPermissionQueryService userPermissionQueryService,
            IUserDeviceQueryService userDeviceQueryService,
            INotificationFactory notificationFactory,
            ISchoolQueryService schoolQueryService,
            ISchoolAccessValidator schoolAccessValidator)
        {
            this.studentAbsencePermitCommandService = studentAbsencePermitCommandService;
            this.studentAbsencePermitQueryService = studentAbsencePermitQueryService;
            this.studentQueryService = studentQueryService;
            this.userPermissionQueryService = userPermissionQueryService;
            this.userDeviceQueryService = userDeviceQueryService;
            this.notificationFactory = notificationFactory;
            this.schoolQueryService = schoolQueryService;
            this.schoolAccessValidator = schoolAccessValidator;
        }

        private Guid CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub) 
                                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;
            }
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

                if (permitDTO.ParentId != CurrentUserId || !await schoolAccessValidator.CanAccessStudentAsync(CurrentUserId, permitDTO.StudentId))
                {
                    return Forbid();
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
                            var userIdsList = userIds?.ToList() ?? new List<Guid>();

                            var school = await schoolQueryService.GetByIdAsync(student.SchoolId);
                            if (school != null && !userIdsList.Contains(school.SchoolAdminId))
                            {
                                userIdsList.Add(school.SchoolAdminId);
                            }

                            if (userIdsList.Any())
                            {
                                var devices = await userDeviceQueryService.GetUsersDevicesAsync(userIdsList);
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

                if (permitsDTO.StudentId.HasValue && !await schoolAccessValidator.CanAccessStudentAsync(CurrentUserId, permitsDTO.StudentId.Value))
                {
                    return Forbid();
                }

                if (permitsDTO.ParentId.HasValue && permitsDTO.ParentId.Value != CurrentUserId)
                {
                    return Forbid();
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
                if (dto.ParentId != CurrentUserId)
                {
                    return Forbid();
                }

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
                if (dto.ReviewerId != CurrentUserId)
                {
                    return Forbid();
                }

                var permit = await studentAbsencePermitQueryService.GetByIdAsync(dto.StudentAbsencePermitId);
                if (permit == null)
                {
                    return BadRequest("Absence Permit Not Found");
                }

                var student = await studentQueryService.GetStudentByIdAsync(permit.StudentId);
                if (student == null)
                {
                    return BadRequest("Student Not Found");
                }

                if (!await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, student.SchoolId))
                {
                    return Forbid();
                }

                var result = await studentAbsencePermitCommandService.ReviewAbsencePermitAsync(dto);

                if (result == "Absence Permit Reviewed")
                {
                    try
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
