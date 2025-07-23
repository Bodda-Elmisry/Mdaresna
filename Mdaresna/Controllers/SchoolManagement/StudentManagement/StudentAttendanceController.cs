using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO;
using Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Command;
using Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Query;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.StudentManagement
{
    [Route("Attendance")]
    public class StudentAttendanceController : Controller
    {
        private readonly IStudentAttendanceCommandService studentAttendanceCommandService;
        private readonly IStudentAttendanceQueryService studentAttendanceQueryService;
        private readonly IStudentQueryService studentQueryService;
        private readonly INotificationFactory notificationFactory;
        private readonly IStudentTransactionsFactory studentTransactionsFactory;

        public StudentAttendanceController(IStudentAttendanceCommandService studentAttendanceCommandService,
                                           IStudentAttendanceQueryService studentAttendanceQueryService,
                                           IStudentQueryService studentQueryService,
                                           INotificationFactory notificationFactory,
                                           IStudentTransactionsFactory studentTransactionsFactory)
        {
            this.studentAttendanceCommandService = studentAttendanceCommandService;
            this.studentAttendanceQueryService = studentAttendanceQueryService;
            this.studentQueryService = studentQueryService;
            this.notificationFactory = notificationFactory;
            this.studentTransactionsFactory = studentTransactionsFactory;
        }

        [HttpPost("GetStudentsAttendences")]
        public async Task<IActionResult> GetStudentsAttendencesList([FromBody] GetStudentsAttendencesDTO studentsAttendencesDTO)
        {
            try
            {
                if(studentsAttendencesDTO.StudentId == null && studentsAttendencesDTO.ClassRoomId == null)
                {
                    return BadRequest("Can't get data without student or class room");
                }

                var data = await studentAttendanceQueryService.GetStudentsAttendancesAsync(studentsAttendencesDTO.StudentId, studentsAttendencesDTO.ClassRoomId, studentsAttendencesDTO.PageNumber);

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SaveAttendance")]
        public async Task<IActionResult> AddClassRoomAttendence([FromBody] AddClassRoomAttendanceDTO attendanceDTO)
        {
            try
            {
                var attendenceCompleated = await studentAttendanceCommandService.AddClassRoomAttendance(attendanceDTO);
                if (attendenceCompleated)
                {
                    foreach (var studentAttendance in attendanceDTO.StudentsAttenndaceList.Where(a=> !a.IsAttend).ToList())
                    {
                        var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                        var studentProvider = studentTransactionsFactory.GetProvider(StudentTransactionProvidersEnum.Attendance);
                        var studentIds = new List<Guid>();
                        studentIds.Add(studentAttendance.StudentId);
                        var devices = await studentProvider.GetTransactionSTudentsParentsDevicesAsync(studentIds);
                        if (devices.Count() > 0)
                        {
                            var tokens = devices.Select(d => d.FcmTocken).ToList();
                            var student = await studentQueryService.GetByIdAsync(studentAttendance.StudentId);
                            await notificationProvider.SendToMultiUsersAsync(tokens, "Attendance", $"Your chield {student.FirstName} {student.LastName} didn't attend");
                        }
                    }
                    return Ok("Attendence Compleated");
                }

                return BadRequest("Errro in attendence");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteStudentAttendence")]
        public async Task<IActionResult> SoftDeleteStudentAttendence([FromBody] StudentAttendanceIdDTO dto)
        {
            try
            {
                var studentAttendence = await studentAttendanceQueryService.GetByIdAsync(dto.StudentAttendanceId);

                if (studentAttendence == null)
                    return BadRequest("There is no attendence to delete");

                studentAttendence.Deleted = true;
                var deleted = studentAttendanceCommandService.Update(studentAttendence);

                return deleted ? Ok("Attendance Deleted") : BadRequest("Error in deleting attendance");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
