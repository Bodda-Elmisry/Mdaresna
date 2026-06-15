using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO;
using Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Command;
using Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Query;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Mdaresna.Middlewares;

namespace Mdaresna.Controllers.SchoolManagement.StudentManagement
{
    [Authorize]
    [Route("ClassRoomStudentAssignment")]
    public class ClassRoomStudentAssignmentController : Controller
    {
        private readonly IClassRoomStudentAssignmentQueryService classRoomStudentAssignmentQueryService;
        private readonly IClassRoomStudentAssignmentCommandService classRoomStudentAssignmentCommandService;
        private readonly IClassRoomAssignmentCommandService classRoomAssignmentCommandService;
        private readonly IStudentQueryService studentQueryService;
        private readonly INotificationFactory notificationFactory;
        private readonly IStudentTransactionsFactory studentTransactionsFactory;

        public ClassRoomStudentAssignmentController(IClassRoomStudentAssignmentQueryService classRoomStudentAssignmentQueryService,
                                                    IClassRoomStudentAssignmentCommandService classRoomStudentAssignmentCommandService,
                                                    IClassRoomAssignmentCommandService classRoomAssignmentCommandService,
                                                  IStudentQueryService studentQueryService,
                                           INotificationFactory notificationFactory,
                                           IStudentTransactionsFactory studentTransactionsFactory)
        {
            this.classRoomStudentAssignmentQueryService = classRoomStudentAssignmentQueryService;
            this.classRoomStudentAssignmentCommandService = classRoomStudentAssignmentCommandService;
            this.classRoomAssignmentCommandService = classRoomAssignmentCommandService;
            this.studentQueryService = studentQueryService;
            this.notificationFactory = notificationFactory;
            this.studentTransactionsFactory = studentTransactionsFactory;
        }

        [HttpPost("GetStudentAssignmentsList")]
        public async Task<IActionResult> GetStudentAssignemtns([FromBody] GetClassRoomStudentAssignmentListDTO dto)
        {
            try
            {
                var list =  await classRoomStudentAssignmentQueryService.GetStudentAssignmentsListAsync(dto.StudentId,
                                                                                                 dto.AssignementId,
                                                                                                 dto.ResultFrom,
                                                                                                 dto.ResultTo,
                                                                                                 dto.IsDelivered,
                                                                                                 dto.DeliveredDateFrom,
                                                                                                 dto.DeliveredDateTo,
                                                                                                 dto.pageNumber);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetStudentAssignment")]
        public async Task<IActionResult> GetStudentAssignemtn([FromBody] GetClassRoomStudentAssignmentDTO dto)
        {
            try
            {
                var list = await classRoomStudentAssignmentQueryService.GetClassRoomStudentAssignmentViewAsync(dto.StudentId,
                                                                                                 dto.AssignementId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [PermissionAuthorize("AddHomework")]
        [HttpPost("AddStudentAssignmnet")]
        public async Task<IActionResult> AddStudentAssignment([FromBody] CreateClassRoomStudentAssignmentDTO dto)
        {
            try
            {
                //var assignment = await classRoomStudentAssignmentQueryService.GetClassRoomStudentAssignmentViewAsync(dto.StudentId, dto.AssignmentId);

                //if (assignment == null)
                //    return BadRequest("This assignment already related to the student");

                var newAssignment = new ClassRoomAssignment
                {
                    AssignmentDate = dto.AssignmentDate,
                    ClassRoomId = dto.ClassRoomId,
                    CourseId = dto.CourseId,
                    Details = dto.Details,
                    Rate = dto.Rate,
                    SupervisorId = dto.SupervisorId,
                    WeekDay = dto.WeekDay
                };

                var assAdded = classRoomAssignmentCommandService.Create(newAssignment);

                if (!assAdded)
                    return BadRequest("Error in adding student assignment");

                var studentAssignment = new ClassRoomStudentAssignment
                {
                    AssignmentId = newAssignment.Id,
                    StudentId = dto.StudentId,
                    Result = dto.Result,
                    IsDelivered = dto.IsDelivered,
                    DeliveredDate = dto.DeliveredDate,

                };

                var sAssAdded = classRoomStudentAssignmentCommandService.Create(studentAssignment);

                if(!sAssAdded)
                    return BadRequest("Error in adding student assignment");

                try
                {
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var studentProvider = studentTransactionsFactory.GetProvider(StudentTransactionProvidersEnum.Assignment);
                    var studentIds = new List<Guid> { dto.StudentId };
                    var devices = await studentProvider.GetTransactionSTudentsParentsDevicesAsync(studentIds);
                    if (devices != null && devices.Any())
                    {
                        var tokens = devices
                            .Select(d => d.FcmTocken)
                            .Where(t => !string.IsNullOrWhiteSpace(t))
                            .Distinct()
                            .ToList();

                        if (tokens.Any())
                        {
                            var student = await studentQueryService.GetByIdAsync(dto.StudentId);
                            var viewDto = await classRoomStudentAssignmentQueryService.GetClassRoomStudentAssignmentViewAsync(studentAssignment.StudentId, studentAssignment.AssignmentId);
                            var classRoomId = viewDto?.ClassRoomId ?? Guid.Empty;
                            var message = $"تمت إضافة واجب جديد لـ {student.FirstName} {student.LastName}. بلمسة من تشجيعكم ومتابعتكم، سيبدع بالتأكيد في إنجازه. | Type=Assignment | TargetId={studentAssignment.AssignmentId} | StudentId={dto.StudentId} | ClassRoomId={classRoomId}";
                            await notificationProvider.SendToMultiUsersAsync(tokens, "واجب مدرسي جديد", message);
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore notification failures so the API request is not aborted
                }

                return Ok(await classRoomStudentAssignmentQueryService.GetClassRoomStudentAssignmentViewAsync(studentAssignment.StudentId, studentAssignment.AssignmentId));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [PermissionAuthorize("RateHomework")]
        [HttpPost("UpdateStudentAssignmnet")]
        public async Task<IActionResult> UpdateStudentAssignment([FromBody] UpdateClassRoomStudentAssignmentDTO dto)
        {
            try
            {
                var sAss = await classRoomStudentAssignmentQueryService.GetClassRoomStudentAssignmentAsync(dto.StudentId, dto.AssignmentId);

                if (sAss == null)
                    return BadRequest("There is no assignment to update");

                sAss.Result = dto.Result;
                sAss.IsDelivered = dto.IsDelivered;
                sAss.DeliveredDate = dto.DeliveredDate;

                var updated = classRoomStudentAssignmentCommandService.Update(sAss);
                if (!updated)
                    return BadRequest("Error in Updating");

                try
                {
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var studentProvider = studentTransactionsFactory.GetProvider(StudentTransactionProvidersEnum.Assignment);
                    var studentIds = new List<Guid> { dto.StudentId };
                    var devices = await studentProvider.GetTransactionSTudentsParentsDevicesAsync(studentIds);
                    if (devices != null && devices.Any())
                    {
                        var tokens = devices
                            .Select(d => d.FcmTocken)
                            .Where(t => !string.IsNullOrWhiteSpace(t))
                            .Distinct()
                            .ToList();

                        if (tokens.Any())
                        {
                            var student = await studentQueryService.GetByIdAsync(dto.StudentId);
                            var viewDto = await classRoomStudentAssignmentQueryService.GetClassRoomStudentAssignmentViewAsync(sAss.StudentId, sAss.AssignmentId);
                            var classRoomId = viewDto?.ClassRoomId ?? Guid.Empty;
                            var message = $"تم تقييم الواجب الخاص بـ {student.FirstName} {student.LastName}. يمكنك الآن الاطلاع على النتائج وملاحظات المعلم عبر التطبيق. | Type=Assignment | TargetId={sAss.AssignmentId} | StudentId={dto.StudentId} | ClassRoomId={classRoomId}";
                            await notificationProvider.SendToMultiUsersAsync(tokens, "تقييم واجب مدرسي", message);
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore notification failures so the API request is not aborted
                }

                return Ok(await classRoomStudentAssignmentQueryService.GetClassRoomStudentAssignmentViewAsync(sAss.StudentId, sAss.AssignmentId));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteStudentAssignment")]
        public async Task<IActionResult> SoftDeleteStudentAssignment([FromBody] GetClassRoomStudentAssignmentDTO dto)
        {
            try
            {
                var studentAssignment = await classRoomStudentAssignmentQueryService.GetClassRoomStudentAssignmentAsync(dto.StudentId, dto.AssignementId);

                if (studentAssignment == null)
                    return BadRequest("There is no assignment to delete");

                studentAssignment.Deleted = true;
                var deleted = classRoomStudentAssignmentCommandService.Update(studentAssignment);

                return deleted ? Ok("Assignment Deleted") : BadRequest("Error in deleting assignment");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




    }
}
