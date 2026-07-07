using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.ClassRoomManagementDTO;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using Mdaresna.Repository.IUnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Mdaresna.Middlewares;

namespace Mdaresna.Controllers.SchoolManagement.ClassRoomManagement
{
    [Authorize]
    [Route("ClassRoomAssignment")]
    public class ClassRoomAssignmentController : Controller
    {
        private readonly IClassRoomAssignmentCommandService classRoomAssignmentCommandService;
        private readonly IClassRoomAssignmentQueryService classRoomAssignmentQueryService;
        private readonly IClassRoomStudentAssignmentCommandService classRoomStudentAssignmentCommandService;
        private readonly IClassRoomStudentAssignmentQueryService classRoomStudentAssignmentQueryService;
        private readonly INotificationFactory notificationFactory;
        private readonly IClassroomTransactionsFactory classroomTransactionsFactory;
        private readonly ICommandUnitOfWork commandUnitOfWork;

        public ClassRoomAssignmentController(IClassRoomAssignmentCommandService classRoomAssignmentCommandService,
                                             IClassRoomAssignmentQueryService classRoomAssignmentQueryService,
                                             IClassRoomStudentAssignmentCommandService classRoomStudentAssignmentCommandService,
                                             IClassRoomStudentAssignmentQueryService classRoomStudentAssignmentQueryService,
                                             INotificationFactory notificationFactory,
                                             IClassroomTransactionsFactory classroomTransactionsFactory,
                                             ICommandUnitOfWork commandUnitOfWork)
        {
            this.classRoomAssignmentCommandService = classRoomAssignmentCommandService;
            this.classRoomAssignmentQueryService = classRoomAssignmentQueryService;
            this.classRoomStudentAssignmentCommandService = classRoomStudentAssignmentCommandService;
            this.classRoomStudentAssignmentQueryService = classRoomStudentAssignmentQueryService;
            this.notificationFactory = notificationFactory;
            this.classroomTransactionsFactory = classroomTransactionsFactory;
            this.commandUnitOfWork = commandUnitOfWork;
        }

        [HttpPost("GetClassroomAssignmentsList")]
        public async Task<IActionResult> GetClassroomAssignmentsList([FromBody] GetClassRoomAssignmentListDTO dTO)
        {
            try
            {
                var items = await classRoomAssignmentQueryService.GetClassRoomAssignmentsList(dTO.classRoomId,
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

        [HttpPost("GetClassroomAssignmentbyid")]
        public async Task<IActionResult> GetClassroomAssignmentById([FromBody] AssignmentIdDTO dTO)
        {
            try
            {
                var item = await classRoomAssignmentQueryService.GetClassRoomAssignmentById(dTO.AssignmentId);

                return Ok(item);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [PermissionAuthorize("AddHomework")]
        [HttpPost("CreateClassRoomAssignment")]
        public async Task<IActionResult> CreateClassRoomAssignment([FromBody] CreateClassRoomAssignmentDTO dTO)
        {
            try
            {
                if (dTO.StudentIds == null || !dTO.StudentIds.Any())
                {
                    return BadRequest("Student list cannot be empty. At least one student must be specified.");
                }

                await commandUnitOfWork.BeginTransactionAsync();

                var assingment = new ClassRoomAssignment
                {
                    AssignmentDate = dTO.AssignmentDate,
                    ClassRoomId = dTO.ClassRoomId,
                    CourseId = dTO.CourseId,
                    Details = dTO.Details,
                    Rate = dTO.Rate,
                    SupervisorId = dTO.SupervisorId,
                    WeekDay = dTO.WeekDay
                };

                var added = await classRoomAssignmentCommandService.Create(assingment, dTO.StudentIds);

                if (added)
                {
                    await commandUnitOfWork.CommitTransactionAsync();

                    try
                    {
                        var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                        var transactionProvider = classroomTransactionsFactory.GetProvider(ClassroomTransactionProvidersEnum.Assignment);
                        var devices = await transactionProvider.GetTransactionSTudentsParentsDevicesAsync(assingment.Id);
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
                                    var message = $"تمت إضافة واجب جديد لـ {childName}. بلمسة من تشجيعكم ومتابعتكم، سيبدع بالتأكيد في إنجازه. | Type=Assignment | TargetId={assingment.Id} | ClassRoomId={assingment.ClassRoomId} | StudentId={devicesGroup.Key}";
                                    await notificationProvider.SendToMultiUsersAsync(tokens, "واجب مدرسي جديد", message);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore notification failures so the API request is not aborted
                    }

                    return Ok(await classRoomAssignmentQueryService.GetClassRoomAssignmentById(assingment.Id));
                }

                await commandUnitOfWork.RollbackTransactionAsync();
                return BadRequest("Error in creating assignment");
            }
            catch (Exception ex)
            {
                await commandUnitOfWork.RollbackTransactionAsync();
                return BadRequest(ex.Message);
            }
        }

        [PermissionAuthorize("UpdateHomework")]
        [HttpPost("UpdateClassRoomAssignment")]
        public async Task<IActionResult> UpdateClassRoomAssignment([FromBody] UpdateClassRoomAssignmentDTO dTO)
        {
            try
            {
                await commandUnitOfWork.BeginTransactionAsync();
                var assignemt = await classRoomAssignmentQueryService.GetByIdAsync(dTO.Id);

                if (assignemt == null)
                {
                    await commandUnitOfWork.RollbackTransactionAsync();
                    return BadRequest("Can't fiend assignment to update");
                }

                assignemt.AssignmentDate = dTO.AssignmentDate;
                assignemt.ClassRoomId = dTO.ClassRoomId;
                assignemt.CourseId = dTO.CourseId;
                assignemt.Details = dTO.Details;
                assignemt.Rate = dTO.Rate;
                assignemt.SupervisorId = dTO.SupervisorId;
                assignemt.WeekDay = dTO.WeekDay;

                var updated = classRoomAssignmentCommandService.Update(assignemt);

                if (!updated)
                {
                    await commandUnitOfWork.RollbackTransactionAsync();
                    return BadRequest("Error in updating assignment");
                }

                var studentIds = dTO.StudentIds?.Distinct().ToList() ?? new List<Guid>();
                var currentAssignments = await classRoomStudentAssignmentQueryService.GetStudentAssignmentsListAsync(dTO.Id);
                var currentStudentIds = currentAssignments.Select(a => a.StudentId).ToHashSet();
                var newStudentIds = studentIds.ToHashSet();

                foreach (var studentId in studentIds.Where(id => !currentStudentIds.Contains(id)))
                {
                    var newAssignment = new ClassRoomStudentAssignment
                    {
                        StudentId = studentId,
                        AssignmentId = dTO.Id,
                        CreateDate = DateTime.Now,
                        Result = 0,
                        IsDelivered = null,
                        DeliveredDate = null
                    };

                    classRoomStudentAssignmentCommandService.Create(newAssignment);
                }

                foreach (var assignmentStudent in currentAssignments.Where(a => !newStudentIds.Contains(a.StudentId)))
                {
                    assignmentStudent.Deleted = true;
                    classRoomStudentAssignmentCommandService.Update(assignmentStudent);
                }

                await commandUnitOfWork.CommitTransactionAsync();
                return Ok(await classRoomAssignmentQueryService.GetClassRoomAssignmentById(dTO.Id));

            }
            catch (Exception ex)
            {
                await commandUnitOfWork.RollbackTransactionAsync();
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteClassroomAssignment")]
        public async Task<IActionResult> SoftDeleteClassroomAssignment([FromBody] AssignmentIdDTO dTO)
        {
            try
            {
                var assignment = await classRoomAssignmentQueryService.GetByIdAsync(dTO.AssignmentId);

                if (assignment == null)
                    return BadRequest("There is no assignment to delete");
                assignment.Deleted = true;

                var assignmentStudents = await classRoomStudentAssignmentQueryService.GetStudentAssignmentsListAsync(assignment.Id);

                foreach (var assignmentStudent in assignmentStudents)
                {
                    assignmentStudent.Deleted = true;
                    classRoomStudentAssignmentCommandService.Update(assignmentStudent);
                }



                var result = classRoomAssignmentCommandService.Update(assignment);

                return result ? Ok("Assignment Deleted") : BadRequest("Error in deleting assignment");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }











    }
}
