using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Enums;
using Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO;
using Mdaresna.Middlewares;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.StudentManagement
{
    [Authorize]
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

        [PermissionAuthorize("AttendanceAction")]
        [HttpPost("SaveAttendance")]
        public async Task<IActionResult> AddClassRoomAttendence([FromBody] AddClassRoomAttendanceDTO attendanceDTO)
        {
            try
            {
                var attendenceCompleated = await studentAttendanceCommandService.AddClassRoomAttendance(attendanceDTO);
                if (attendenceCompleated)
                {
                    try
                    {
                        var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                        var studentProvider = studentTransactionsFactory.GetProvider(StudentTransactionProvidersEnum.Attendance);

                        foreach (var studentAttendance in attendanceDTO.StudentsAttenndaceList)
                        {
                            try
                            {
                                var studentIds = new List<Guid> { studentAttendance.StudentId };
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
                                        var student = await studentQueryService.GetByIdAsync(studentAttendance.StudentId);
                                        var messageText = studentAttendance.IsAttend
                                            ? $"تم تسجيل حضور الطالب {student.FirstName} {student.LastName} في المدرسة. نتمنى له يوماً دراسياً مليئاً بالنشاط والتميز!"
                                            : $"نود إفادتكم بأن الطالب {student.FirstName} {student.LastName} غائب عن مقعده الدراسي اليوم. نأمل أن يكون المانع خيراً، مع تمنياتنا له بالسلامة.";

                                        var message = $"{messageText} | Type=Attendance | StudentId={studentAttendance.StudentId} | ClassRoomId={attendanceDTO.ClassRoomId}";
                                        await notificationProvider.SendToMultiUsersAsync(tokens, "تسجيل الحضور", message);
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // Ignore failure for individual student notification
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore notification failures so the API request is not aborted
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
