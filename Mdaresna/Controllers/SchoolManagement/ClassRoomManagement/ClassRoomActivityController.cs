using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.ClassRoomManagementDTO;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Mdaresna.Middlewares;

namespace Mdaresna.Controllers.SchoolManagement.ClassRoomManagement
{
    [Authorize]
    [Route("ClassRoomActivity")]
    public class ClassRoomActivityController : Controller
    {
        private readonly IClassRoomActivityQueryService classRoomActivityQueryService;
        private readonly IClassRoomActivityCommandService classRoomActivityCommandService;
        private readonly IClassRoomStudentActivityQueryService classRoomStudentActivityQueryService;
        private readonly IClassRoomStudentActivityCommandService classRoomStudentActivityCommandService;
        private readonly INotificationFactory notificationFactory;
        private readonly IClassroomTransactionsFactory classroomTransactionsFactory;

        public ClassRoomActivityController(IClassRoomActivityQueryService classRoomActivityQueryService,
                                           IClassRoomActivityCommandService classRoomActivityCommandService,
                                           IClassRoomStudentActivityQueryService classRoomStudentActivityQueryService,
                                           IClassRoomStudentActivityCommandService classRoomStudentActivityCommandService,
                                           INotificationFactory notificationFactory,
                                           IClassroomTransactionsFactory classroomTransactionsFactory)
        {
            this.classRoomActivityQueryService = classRoomActivityQueryService;
            this.classRoomActivityCommandService = classRoomActivityCommandService;
            this.classRoomStudentActivityQueryService = classRoomStudentActivityQueryService;
            this.classRoomStudentActivityCommandService = classRoomStudentActivityCommandService;
            this.notificationFactory = notificationFactory;
            this.classroomTransactionsFactory = classroomTransactionsFactory;
        }

        [HttpPost("GetClassroomActivitysList")]
        public async Task<IActionResult> GetClassroomActivitysList([FromBody] GetClassRoomActivityDTO dTO)
        {
            try
            {
                var items = await classRoomActivityQueryService.GetClassRoomActivitiesList(dTO.classRoomId,
                                                                                              dTO.SupervisorId,
                                                                                              dTO.courseId,
                                                                                              dTO.details,
                                                                                              dTO.rate,
                                                                                              dTO.fromdate,
                                                                                              dTO.todate,
                                                                                              dTO.pageNumber);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetClassroomActivitybyid")]
        public async Task<IActionResult> GetClassroomActivityById([FromBody] ActivityIdDTO dTO)
        {
            try
            {
                var item = await classRoomActivityQueryService.GetClassRoomActivityById(dTO.ActivityId);

                return Ok(item);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [PermissionAuthorize("AddActivity")]
        [HttpPost("CreateClassRoomActivity")]
        public async Task<IActionResult> CreateClassRoomAssignement([FromBody] CreateClassRoomActivityDTO dTO)
        {
            try
            {
                var activity = new ClassRoomActivity
                {
                    ActivityDate = dTO.ActivityDate,
                    ClassRoomId = dTO.ClassRoomId,
                    CourseId = dTO.CourseId,
                    Details = dTO.Details,
                    Rate = dTO.Rate,
                    SupervisorId = dTO.SupervisorId,
                    WeekDay = dTO.WeekDay
                };

                var added = await classRoomActivityCommandService.Create(activity, dTO.StudentIds);

                if (added)
                {
                    try
                    {
                        var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                        var transactionProvider = classroomTransactionsFactory.GetProvider(ClassroomTransactionProvidersEnum.Activity);
                        var devices = await transactionProvider.GetTransactionSTudentsParentsDevicesAsync(activity.Id);
                        if (devices != null && devices.Any())
                        {
                            foreach (var devicesGroup in devices.GroupBy(d => d.StudentId))
                            {
                                var tokens = devicesGroup
                                    .Select(d => d.FcmTocken)
                                    .Where(t => !string.IsNullOrWhiteSpace(t))
                                    .Distinct()
                                    .ToList();

                                if (tokens.Any())
                                {
                                    var childName = devicesGroup.FirstOrDefault()?.StudentName ?? "";
                                    var message = $"تمت إضافة نشاط جديد لـ {childName}. مشاركتكم تشجعهم على التميز والإبداع. | Type=Activity | TargetId={activity.Id} | ClassRoomId={activity.ClassRoomId} | StudentId={devicesGroup.Key}";
                                    await notificationProvider.SendToMultiUsersAsync(tokens, "نشاط مدرسي جديد", message);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore notification failures so the API request is not aborted
                    }

                    return Ok(await classRoomActivityQueryService.GetClassRoomActivityById(activity.Id));
                }

                return BadRequest("Error in creating assignement");


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [PermissionAuthorize("UpdateActivity")]
        [HttpPost("UpdateClassRoomActivity")]
        public async Task<IActionResult> UpdateClassRoomAssignement([FromBody] UpdateClassRoomActivityDTO dTO)
        {
            try
            {
                var activity = await classRoomActivityQueryService.GetByIdAsync(dTO.Id);

                if (activity == null)
                    return BadRequest("Can't fiend activity to update");

                activity.ActivityDate = dTO.ActivityDate;
                activity.ClassRoomId = dTO.ClassRoomId;
                activity.CourseId = dTO.CourseId;
                activity.Details = dTO.Details;
                activity.Rate = dTO.Rate;
                activity.SupervisorId = dTO.SupervisorId;
                activity.WeekDay = dTO.WeekDay;

                var updated = classRoomActivityCommandService.Update(activity);

                if (updated)
                    return Ok(await classRoomActivityQueryService.GetClassRoomActivityById(dTO.Id));

                return BadRequest("Error in updating activity");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteClassroomActivity")]
        public async Task<IActionResult> SoftDeleteClassroomActivity([FromBody] ActivityIdDTO dTO)
        {
            try
            {
                var activity = await classRoomActivityQueryService.GetByIdAsync(dTO.ActivityId);

                if (activity == null)
                    return BadRequest("There is no activity to delete");

                activity.Deleted = true;

                var activityStudents = await classRoomStudentActivityQueryService.GetStudentActivitiesListAsync(activity.Id);

                foreach (var activityStudent in activityStudents)
                {
                    activityStudent.Deleted = true;
                    classRoomStudentActivityCommandService.Update(activityStudent);
                }

                

                var result = classRoomActivityCommandService.Update(activity);

                return result ? Ok("Activity Deleted") : BadRequest("Error in deleting activity");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
