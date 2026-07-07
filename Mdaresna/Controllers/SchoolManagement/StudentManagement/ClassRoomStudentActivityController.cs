using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO;
using Mdaresna.Infrastructure.Services.SchoolManagement.ClassRoomManagement.Command;
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
    [Route("ClassRoomStudentActivity")]
    public class ClassRoomStudentActivityController : Controller
    {
        private readonly IClassRoomStudentActivityQueryService classRoomStudentActivityQueryService;
        private readonly IClassRoomStudentActivityCommandService classRoomStudentActivityCommandService;
        private readonly IClassRoomActivityCommandService classRoomActivityCommandService;
        private readonly IStudentQueryService studentQueryService;
        private readonly INotificationFactory notificationFactory;
        private readonly IStudentTransactionsFactory studentTransactionsFactory;

        public ClassRoomStudentActivityController(IClassRoomStudentActivityQueryService classRoomStudentActivityQueryService,
                                                  IClassRoomStudentActivityCommandService classRoomStudentActivityCommandService,
                                                  IClassRoomActivityCommandService classRoomActivityCommandService,
                                                  IStudentQueryService studentQueryService,
                                                  INotificationFactory notificationFactory,
                                                  IStudentTransactionsFactory studentTransactionsFactory)
        {
            this.classRoomStudentActivityQueryService = classRoomStudentActivityQueryService;
            this.classRoomStudentActivityCommandService = classRoomStudentActivityCommandService;
            this.classRoomActivityCommandService = classRoomActivityCommandService;
            this.studentQueryService = studentQueryService;
            this.notificationFactory = notificationFactory;
            this.studentTransactionsFactory = studentTransactionsFactory;
        }

        [HttpPost("GetStudentActivitiesList")]
        public async Task<IActionResult> GetStudentActivities([FromBody] GetClassRoomStudentActivityListDTO dto)
        {
            try
            {
                var list = await classRoomStudentActivityQueryService.GetStudentActivitiesListAsync(dto.StudentId,
                                                                                                    dto.ActivityId,
                                                                                                    dto.ResultFrom,
                                                                                                    dto.ResultTo,
                                                                                                    dto.pageNumber);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetStudentActivity")]
        public async Task<IActionResult> GetStudentActivity([FromBody] GetClassRoomStudentActivityDTO dto)
        {
            try
            {
                var list = await classRoomStudentActivityQueryService.GetClassRoomStudentActivityViewAsync(dto.StudentId,
                                                                                                           dto.ActivityId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [PermissionAuthorize("AddActivity")]
        [HttpPost("AddStudentActivity")]
        public async Task<IActionResult> AddStudentActivity([FromBody] CreateClassRoomStudentActivityDTO dto)
        {
            try
            {
                //var assignment = await classRoomStudentAssignmentQueryService.GetClassRoomStudentAssignmentViewAsync(dto.StudentId, dto.AssignmentId);

                //if (assignment == null)
                //    return BadRequest("This assignment already related to the student");

                var newActivity = new ClassRoomActivity
                {
                    ActivityDate = dto.ActivityDate,
                    ClassRoomId = dto.ClassRoomId,
                    CourseId = dto.CourseId,
                    Details = dto.Details,
                    Rate = dto.ActivityRate,
                    SupervisorId = dto.SupervisorId,
                    WeekDay = dto.WeekDay
                };

                var actAdded = classRoomActivityCommandService.Create(newActivity);

                if (!actAdded)
                    return BadRequest("Error in adding student activity");

                var studentActivity = new ClassRoomStudentActivity
                {
                    ActivityId = newActivity.Id,
                    StudentId = dto.StudentId,
                    Result = dto.Result,
                    IsAttend = dto.IsAttend
                };

                var sAssAdded = classRoomStudentActivityCommandService.Create(studentActivity);

                if (!sAssAdded)
                    return BadRequest("Error in adding student Activity");
                

                try
                {
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var studentProvider = studentTransactionsFactory.GetProvider(StudentTransactionProvidersEnum.Activity);
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
                            var message = $"نشاط مدرسي جديد بانتظار {student.FirstName} {student.LastName}! تعرف على التفاصيل وشاركه الحماس عبر قسم الأنشطة. | Type=Activity | TargetId={studentActivity.ActivityId} | StudentId={dto.StudentId} | ClassRoomId={dto.ClassRoomId}";
                            await notificationProvider.SendToMultiUsersAsync(tokens, "نشاط مدرسي جديد", message);
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore notification failures so the API request is not aborted
                }

                return Ok(await classRoomStudentActivityQueryService.GetClassRoomStudentActivityViewAsync(studentActivity.StudentId, studentActivity.ActivityId));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [PermissionAuthorize("RateActivity")]
        [HttpPost("UpdateStudentActivity")]
        public async Task<IActionResult> UpdateStudentActivity([FromBody] UpdateClassRoomStudentActivityDTO dto)
        {
            try
            {
                var sAct = await classRoomStudentActivityQueryService.GetClassRoomStudentActivityAsync(dto.StudentId, dto.ActivityId);

                if (sAct == null)
                    return BadRequest("There is no activity to update");

                bool wasModified = sAct.Result != dto.Result ||
                                   sAct.IsAttend != dto.IsAttend;

                sAct.Result = dto.Result;
                sAct.IsAttend = dto.IsAttend;

                var updated = classRoomStudentActivityCommandService.Update(sAct);
                if (!updated)
                    return BadRequest("Error in Updating");


                if (wasModified)
                {
                    try
                    {
                        var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                        var studentProvider = studentTransactionsFactory.GetProvider(StudentTransactionProvidersEnum.Activity);
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
                                var viewDto = await classRoomStudentActivityQueryService.GetClassRoomStudentActivityViewAsync(sAct.StudentId, sAct.ActivityId);
                                var classRoomId = viewDto?.ClassRoomId ?? Guid.Empty;
                                var message = $"تم رصد تقييم مشاركة {student.FirstName} {student.LastName} في النشاط. يمكنكم الآن رؤية التقييم والاحتفاء بتجربته الأخيرة. | Type=Activity | TargetId={sAct.ActivityId} | StudentId={dto.StudentId} | ClassRoomId={classRoomId}";
                                await notificationProvider.SendToMultiUsersAsync(tokens, "تقييم نشاط مدرسي", message);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore notification failures so the API request is not aborted
                    }
                }

                return Ok(await classRoomStudentActivityQueryService.GetClassRoomStudentActivityViewAsync(sAct.StudentId, sAct.ActivityId));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteStudentActivity")]
        public async Task<IActionResult> SoftDeleteStudentActivity([FromBody] GetClassRoomStudentActivityDTO dto)
        {
            try
            {
                var studentActivity = await classRoomStudentActivityQueryService.GetClassRoomStudentActivityAsync(dto.StudentId, dto.ActivityId);

                if (studentActivity == null)
                    return BadRequest("There is no activity to delete");

                studentActivity.Deleted = true;
                var deleted = classRoomStudentActivityCommandService.Update(studentActivity);

                return deleted ? Ok("Activity Deleted") : BadRequest("Error in deleting activity");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



    }
}
