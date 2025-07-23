using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.ClassRoomManagementDTO;
using Mdaresna.Infrastructure.Factories;
using Mdaresna.Infrastructure.Services.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Infrastructure.Services.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Command;
using Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Query;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Mdaresna.Controllers.SchoolManagement.ClassRoomManagement
{
    [Route("ClassRoomAssignment")]
    public class ClassRoomAssignmentController : Controller
    {
        private readonly IClassRoomAssignmentCommandService classRoomAssignmentCommandService;
        private readonly IClassRoomAssignmentQueryService classRoomAssignmentQueryService;
        private readonly IClassRoomStudentAssignmentCommandService classRoomStudentAssignmentCommandService;
        private readonly IClassRoomStudentAssignmentQueryService classRoomStudentAssignmentQueryService;
        private readonly INotificationFactory notificationFactory;
        private readonly IClassroomTransactionsFactory classroomTransactionsFactory;

        public ClassRoomAssignmentController(IClassRoomAssignmentCommandService classRoomAssignmentCommandService,
                                             IClassRoomAssignmentQueryService classRoomAssignmentQueryService,
                                             IClassRoomStudentAssignmentCommandService classRoomStudentAssignmentCommandService,
                                             IClassRoomStudentAssignmentQueryService classRoomStudentAssignmentQueryService,
                                           INotificationFactory notificationFactory,
                                           IClassroomTransactionsFactory classroomTransactionsFactory)
        {
            this.classRoomAssignmentCommandService = classRoomAssignmentCommandService;
            this.classRoomAssignmentQueryService = classRoomAssignmentQueryService;
            this.classRoomStudentAssignmentCommandService = classRoomStudentAssignmentCommandService;
            this.classRoomStudentAssignmentQueryService = classRoomStudentAssignmentQueryService;
            this.notificationFactory = notificationFactory;
            this.classroomTransactionsFactory = classroomTransactionsFactory;
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

        [HttpPost("CreateClassRoomAssignment")]
        public async Task<IActionResult> CreateClassRoomAssignment([FromBody] CreateClassRoomAssignmentDTO dTO)
        {
            try
            {
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


                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var transactionProvider = classroomTransactionsFactory.GetProvider(ClassroomTransactionProvidersEnum.Assignment);
                    var devices = await transactionProvider.GetTransactionSTudentsParentsDevicesAsync(assingment.Id);
                    
                    if (devices.Count() > 0)
                    {
                        foreach (var devicesGroup in devices.GroupBy(d => d.StudentId))
                        {
                            var tokens = devicesGroup.Select(d => d.FcmTocken).ToList();
                            var chieldName = devicesGroup.FirstOrDefault().StudentName;
                            await notificationProvider.SendToMultiUsersAsync(tokens, "New Homework", $"New homework added to your chield {chieldName}");
                        }

                        
                    }
                    return Ok(await classRoomAssignmentQueryService.GetClassRoomAssignmentById(assingment.Id));
                }

                return BadRequest("Error in creating assignment");


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateClassRoomAssignment")]
        public async Task<IActionResult> UpdateClassRoomAssignment([FromBody] UpdateClassRoomAssignmentDTO dTO)
        {
            try
            {
                var assignemt = await classRoomAssignmentQueryService.GetByIdAsync(dTO.Id);

                if (assignemt == null)
                    return BadRequest("Can't fiend assignment to update");

                assignemt.AssignmentDate = dTO.AssignmentDate;
                assignemt.ClassRoomId = dTO.ClassRoomId;
                assignemt.CourseId = dTO.CourseId;
                assignemt.Details = dTO.Details;
                assignemt.Rate = dTO.Rate;
                assignemt.SupervisorId = dTO.SupervisorId;
                assignemt.WeekDay = dTO.WeekDay;

                var updated = classRoomAssignmentCommandService.Update(assignemt);

                if (updated)
                    return Ok(await classRoomAssignmentQueryService.GetClassRoomAssignmentById(dTO.Id));

                return BadRequest("Error in updating assignment");

            }
            catch (Exception ex)
            {
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
