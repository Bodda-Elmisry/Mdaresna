using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO;
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
    [Route("ClassRoomStudentExam")]
    public class ClassRoomStudentExamController : Controller
    {
        private readonly IClassRoomStudentExamCommandService classRoomStudentExamCommandService;
        private readonly IClassRoomStudentExamQueryService classRoomStudentExamQueryService;
        private readonly IClassRoomExamCommandService classRoomExamCommandService;
        private readonly IStudentQueryService studentQueryService;
        private readonly INotificationFactory notificationFactory;
        private readonly IStudentTransactionsFactory studentTransactionsFactory;

        public ClassRoomStudentExamController(IClassRoomStudentExamCommandService classRoomStudentExamCommandService,
                                              IClassRoomStudentExamQueryService classRoomStudentExamQueryService,
                                              IClassRoomExamCommandService classRoomExamCommandService,
                                                  IStudentQueryService studentQueryService,
                                           INotificationFactory notificationFactory,
                                           IStudentTransactionsFactory studentTransactionsFactory)
        {
            this.classRoomStudentExamCommandService = classRoomStudentExamCommandService;
            this.classRoomStudentExamQueryService = classRoomStudentExamQueryService;
            this.classRoomExamCommandService = classRoomExamCommandService;
            this.studentQueryService = studentQueryService;
            this.notificationFactory = notificationFactory;
            this.studentTransactionsFactory = studentTransactionsFactory;
        }

        [HttpPost("GetStudentExamsList")]
        public async Task<IActionResult> GetStudentExamsList([FromBody] GetClassRoomStudentExamsListDTO dto)
        {
            try
            {
                var exams = await classRoomStudentExamQueryService.GetClassRoomStudentExamsListAsync(dto.StudentId,
                                                                                                     dto.ExamId,
                                                                                                     dto.TotalResultFrom,
                                                                                                     dto.TotalResultTo,
                                                                                                     dto.IsAttend,
                                                                                                     dto.pageNumber);
                return Ok(exams);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetStudentExam")]
        public async Task<IActionResult> GetStudentExam([FromBody] GetClassRoomStudentExamDTO dto)
        {
            try
            {
                var exams = await classRoomStudentExamQueryService.GetClassRoomStudentExamViewAsync(dto.StudentId,
                                                                                                     dto.ExamId);
                return Ok(exams);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [PermissionAuthorize("AddExam")]
        [HttpPost("AddStudentExam")]
        public async Task<IActionResult> AddStudentExam([FromBody] CreateClassRoomStudentExamDTO dto)
        {
            try
            {
                var exam = new ClassRoomExam
                {
                    ExamDate = dto.ExamDate,
                    WeekDay = dto.WeekDay,
                    Details = dto.ExamDetails,
                    ClassRoomId = dto.ClassRoomId,
                    SupervisorId = dto.SupervisorId,
                    MonthId = dto.MonthId,
                    CourseId = dto.CourseId,
                    Rate = dto.Rate
                };

                var examAdded = classRoomExamCommandService.Create(exam);

                if (!examAdded)
                    return BadRequest("Error in adding student exam");

                var studentExam = new ClassRoomStudentExam
                {
                    ExamId = exam.Id,
                    StudentId = dto.StudentId,
                    IsAttend = dto.IsAttend,
                    TotalResult = dto.TotalResult,
                };

                var studentAdded = classRoomStudentExamCommandService.Create(studentExam);

                if (!studentAdded)
                    return BadRequest("Errro in adding student exam");

                try
                {
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var studentProvider = studentTransactionsFactory.GetProvider(StudentTransactionProvidersEnum.Exam);
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
                            var viewDto = await classRoomStudentExamQueryService.GetClassRoomStudentExamViewAsync(studentExam.StudentId, studentExam.ExamId);
                            var classRoomId = viewDto?.ClassRoomId ?? Guid.Empty;
                            var message = $"تمت إضافة اختبار جديد لـ {student.FirstName} {student.LastName}. تفاصيل المادة والموعد جاهزة للاطلاع.. كل التوفيق والنجاح له | Type=Exam | TargetId={studentExam.ExamId} | StudentId={dto.StudentId} | ClassRoomId={classRoomId}";
                            await notificationProvider.SendToMultiUsersAsync(tokens, "اختبار جديد", message);
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore notification failures so the API request is not aborted
                }

                return Ok(await classRoomStudentExamQueryService.GetClassRoomStudentExamViewAsync(studentExam.StudentId, studentExam.ExamId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AssignStudentToExam")]
        public async Task<IActionResult> AssignStudentToExam([FromBody] AssignClassRoomStudentExamDTO dto)
        {
            try
            {
                var studentExam = new ClassRoomStudentExam
                {
                    StudentId = dto.StudentId,
                    ExamId = dto.ExamId,
                    IsAttend = false,
                    TotalResult = null
                };

                var added = classRoomStudentExamCommandService.Create(studentExam);

                if (!added)
                    return BadRequest("Error in assigning student");

                return Ok(await classRoomStudentExamQueryService.GetClassRoomStudentExamViewAsync(dto.StudentId, dto.ExamId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [PermissionAuthorize("RateExam")]
        [HttpPost("UpdateStudentExam")]
        public async Task<IActionResult> UpdateStudentExam([FromBody] UpdateClassRoomStudentExamDTO dto)
        {
            try
            {
                var exam = await classRoomStudentExamQueryService.GetClassRoomStudentExamAsync(dto.StudentId, dto.ExamId);

                if (exam == null)
                    return BadRequest("There is no exam student to update");

                bool wasModified = exam.TotalResult != dto.TotalResult ||
                                   exam.IsAttend != dto.IsAttend;

                exam.TotalResult = dto.TotalResult;
                exam.IsAttend = dto.IsAttend;

                var updated = classRoomStudentExamCommandService.Update(exam);

                if (!updated) 
                    return BadRequest("Error in updating student exam");

                if (wasModified)
                {
                    try
                    {
                        var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                        var studentProvider = studentTransactionsFactory.GetProvider(StudentTransactionProvidersEnum.Exam);
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
                                var viewDto = await classRoomStudentExamQueryService.GetClassRoomStudentExamViewAsync(dto.StudentId, dto.ExamId);
                                var classRoomId = viewDto?.ClassRoomId ?? Guid.Empty;
                                var message = $"ظهرت الآن نتائج اختبار {student.FirstName} {student.LastName} . تفضل بالاطلاع عليها لمتابعة تطوره الدراسي المستمر | Type=Exam | TargetId={dto.ExamId} | StudentId={dto.StudentId} | ClassRoomId={classRoomId}";
                                await notificationProvider.SendToMultiUsersAsync(tokens, "نتائج اختبار", message);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore notification failures so the API request is not aborted
                    }
                }

                return Ok(await classRoomStudentExamQueryService.GetClassRoomStudentExamViewAsync(dto.StudentId, dto.ExamId));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteStudentExam")]
        public async Task<IActionResult> SoftDeleteStudentExam([FromBody] GetClassRoomStudentExamDTO dto)
        {
            try
            {
                var studentExam = await classRoomStudentExamQueryService.GetClassRoomStudentExamAsync(dto.StudentId, dto.ExamId);

                if (studentExam == null)
                    return BadRequest("There is no exam to delete");

                studentExam.Deleted = true;
                var deleted = classRoomStudentExamCommandService.Update(studentExam);

                return deleted ? Ok("Exam Deleted") : BadRequest("Error in deleting exam");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




    }
}
