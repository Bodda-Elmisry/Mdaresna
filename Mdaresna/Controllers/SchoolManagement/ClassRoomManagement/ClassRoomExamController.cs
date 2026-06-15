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

namespace Mdaresna.Controllers.SchoolManagement.ClassRoomManagement
{
    [Route("Exams")]
    public class ClassRoomExamController : Controller
    {
        private readonly IClassRoomExamQueryService classRoomExamQueryService;
        private readonly IClassRoomExamCommandService classRoomExamCommandService;
        private readonly IClassRoomStudentExamCommandService classRoomStudentExamCommandService;
        private readonly IClassRoomStudentExamQueryService classRoomStudentExamQueryService;
        private readonly INotificationFactory notificationFactory;
        private readonly IClassroomTransactionsFactory classroomTransactionsFactory;
        private readonly ICommandUnitOfWork commandUnitOfWork;

        public ClassRoomExamController(IClassRoomExamQueryService classRoomExamQueryService,
                                       IClassRoomExamCommandService classRoomExamCommandService,
                                       IClassRoomStudentExamCommandService classRoomStudentExamCommandService,
                                       IClassRoomStudentExamQueryService classRoomStudentExamQueryService,
                                       INotificationFactory notificationFactory,
                                       IClassroomTransactionsFactory classroomTransactionsFactory,
                                       ICommandUnitOfWork commandUnitOfWork)
        {
            this.classRoomExamQueryService = classRoomExamQueryService;
            this.classRoomExamCommandService = classRoomExamCommandService;
            this.classRoomStudentExamCommandService = classRoomStudentExamCommandService;
            this.classRoomStudentExamQueryService = classRoomStudentExamQueryService;
            this.notificationFactory = notificationFactory;
            this.classroomTransactionsFactory = classroomTransactionsFactory;
            this.commandUnitOfWork = commandUnitOfWork;
        }

        [HttpPost("GetInitailData")]
        public async Task<IActionResult> GetInitialData([FromBody] SchoolIdDTO schoolId)
        {
            try
            {
                var data = await classRoomExamQueryService.GetInitialData(schoolId.SchoolId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetExams")]
        public async Task<IActionResult> GetExams([FromBody] GetClassRoomExamsDTO classRoomExamsDTO)
        {
            try
            {
                var exams = await classRoomExamQueryService.GetExamsList(classRoomExamsDTO.Months,
                                                                         classRoomExamsDTO.FromDate,
                                                                         classRoomExamsDTO.ToDate,
                                                                         classRoomExamsDTO.WeekDay,
                                                                         classRoomExamsDTO.ClassRoomId,
                                                                         classRoomExamsDTO.SupervisorId,
                                                                         classRoomExamsDTO.CourseId,
                                                                         classRoomExamsDTO.Rate,
                                                                         classRoomExamsDTO.PageNumber);

                return Ok(exams);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetExamById")]
        public async Task<IActionResult> GetExamById([FromBody] ExamIdDTO examIdDTO)
        {
            try
            {
                var exam = await classRoomExamQueryService.GetExamByIdAsync(examIdDTO.ExamId);

                return Ok(exam);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddNewExam")]
        public async Task<IActionResult> CreateNewExam([FromBody] CreateClassRoomExamDTO classRoomExamDTO)
        {
            try
            {
                var exam = new ClassRoomExam
                {
                    ClassRoomId = classRoomExamDTO.ClassRoomId,
                    CourseId = classRoomExamDTO.CourseId,
                    Details = classRoomExamDTO.ExamDetails,
                    ExamDate = classRoomExamDTO.ExamDate,
                    MonthId = classRoomExamDTO.MonthId,
                    Rate = classRoomExamDTO.Rate,
                    SupervisorId = classRoomExamDTO.SupervisorId,
                    WeekDay = classRoomExamDTO.WeekDay
                };


                var added = await classRoomExamCommandService.Create(exam, classRoomExamDTO.StudentsIds);

                if (added)
                {
                    try
                    {
                        var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                        var transactionProvider = classroomTransactionsFactory.GetProvider(ClassroomTransactionProvidersEnum.Exam);
                        var devices = await transactionProvider.GetTransactionSTudentsParentsDevicesAsync(exam.Id);
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
                                    var message = $"تم تحديد موعد اختبار جديد لـ {childName}. دعواتنا وتوجيهاتكم هي سر نجاحهم وتفوقهم. | Type=Exam | TargetId={exam.Id} | ClassRoomId={exam.ClassRoomId}";
                                    await notificationProvider.SendToMultiUsersAsync(tokens, "اختبار جديد", message);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore notification failures so the API request is not aborted
                    }

                    return Ok(await classRoomExamQueryService.GetExamByIdAsync(exam.Id));
                }

                return BadRequest("Error in adding exam");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("UpdateExam")]
        public async Task<IActionResult> UpdateExam([FromBody] UpdateClassRoomExamDTO classRoomExamDTO)
        {
            try
            {
                await commandUnitOfWork.BeginTransactionAsync();
                var exam = await classRoomExamQueryService.GetByIdAsync(classRoomExamDTO.Id);

                if (exam == null)
                {
                    await commandUnitOfWork.RollbackTransactionAsync();
                    return BadRequest("Can't fiend exam to update");
                }

                exam.ClassRoomId = classRoomExamDTO.ClassRoomId;
                exam.CourseId = classRoomExamDTO.CourseId;
                exam.Details = classRoomExamDTO.ExamDetails;
                exam.ExamDate = classRoomExamDTO.ExamDate;
                exam.MonthId = classRoomExamDTO.MonthId;
                exam.Rate = classRoomExamDTO.Rate;
                exam.SupervisorId = classRoomExamDTO.SupervisorId;
                exam.WeekDay = classRoomExamDTO.WeekDay;

                var updated = classRoomExamCommandService.Update(exam);

                if (!updated)
                {
                    await commandUnitOfWork.RollbackTransactionAsync();
                    return BadRequest("Error in updating exam");
                }

                var studentIds = classRoomExamDTO.StudentsIds?.Distinct().ToList() ?? new List<Guid>();
                var currentExams = await classRoomStudentExamQueryService.GetClassRoomStudentExamsListAsync(exam.Id);
                var currentStudentIds = currentExams.Select(e => e.StudentId).ToHashSet();
                var newStudentIds = studentIds.ToHashSet();

                foreach (var studentId in studentIds.Where(id => !currentStudentIds.Contains(id)))
                {
                    var newExam = new ClassRoomStudentExam
                    {
                        StudentId = studentId,
                        CreateDate = DateTime.Now,
                        ExamId = exam.Id,
                        IsAttend = false,
                        TotalResult = null
                    };

                    classRoomStudentExamCommandService.Create(newExam);
                }

                foreach (var examStudent in currentExams.Where(e => !newStudentIds.Contains(e.StudentId)))
                {
                    examStudent.Deleted = true;
                    classRoomStudentExamCommandService.Update(examStudent);
                }

                await commandUnitOfWork.CommitTransactionAsync();
                return Ok(await classRoomExamQueryService.GetExamByIdAsync(exam.Id));

            }
            catch (Exception ex)
            {
                await commandUnitOfWork.RollbackTransactionAsync();
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteClassroomExam")]
        public async Task<IActionResult> SoftDeleteClassroomExam([FromBody] ExamIdDTO dTO)
        {
            try
            {
                var exam = await classRoomExamQueryService.GetByIdAsync(dTO.ExamId);

                if (exam == null)
                    return BadRequest("There is no exam to delete");

                exam.Deleted = true;

                var examStudents = await classRoomStudentExamQueryService.GetClassRoomStudentExamsListAsync(exam.Id);

                foreach (var examStudent in examStudents)
                {
                    examStudent.Deleted = true;
                    classRoomStudentExamCommandService.Update(examStudent);
                }



                var result = classRoomExamCommandService.Update(exam);

                return result ? Ok("Exam Deleted") : BadRequest("Error in deleting exam");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }












    }
}
