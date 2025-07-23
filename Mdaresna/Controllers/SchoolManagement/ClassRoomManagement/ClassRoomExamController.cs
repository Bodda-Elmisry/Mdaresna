using Mdaresna.Doamin.DTOs.ClassRoomManagement;
using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.ClassRoomManagementDTO;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public ClassRoomExamController(IClassRoomExamQueryService classRoomExamQueryService,
                                       IClassRoomExamCommandService classRoomExamCommandService,
                                       IClassRoomStudentExamCommandService classRoomStudentExamCommandService,
                                       IClassRoomStudentExamQueryService classRoomStudentExamQueryService,
                                           INotificationFactory notificationFactory,
                                           IClassroomTransactionsFactory classroomTransactionsFactory)
        {
            this.classRoomExamQueryService = classRoomExamQueryService;
            this.classRoomExamCommandService = classRoomExamCommandService;
            this.classRoomStudentExamCommandService = classRoomStudentExamCommandService;
            this.classRoomStudentExamQueryService = classRoomStudentExamQueryService;
            this.notificationFactory = notificationFactory;
            this.classroomTransactionsFactory = classroomTransactionsFactory;
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
                                                                         classRoomExamsDTO.Rate);

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
                    var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                    var transactionProvider = classroomTransactionsFactory.GetProvider(ClassroomTransactionProvidersEnum.Exam);
                    var devices = await transactionProvider.GetTransactionSTudentsParentsDevicesAsync(exam.Id);
                    if (devices.Count() > 0)
                    {
                        foreach (var devicesGroup in devices.GroupBy(d => d.StudentId))
                        {
                            var tokens = devicesGroup.Select(d => d.FcmTocken).ToList();
                            var chieldName = devicesGroup.FirstOrDefault().StudentName;
                            await notificationProvider.SendToMultiUsersAsync(tokens, "New Exam", $"New exam added to your chield {chieldName}");
                        }
                        //var tokens = devices.Select(d => d.FcmTocken).ToList();
                        //await notificationProvider.SendToMultiUsersAsync(tokens, "New Exam", "New exam added to your chield");
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
                var exam = await classRoomExamQueryService.GetByIdAsync(classRoomExamDTO.Id);

                if (exam == null)
                    return BadRequest("Can't fiend exam to update");

                exam.ClassRoomId = classRoomExamDTO.ClassRoomId;
                exam.CourseId = classRoomExamDTO.CourseId;
                exam.Details = classRoomExamDTO.ExamDetails;
                exam.ExamDate = classRoomExamDTO.ExamDate;
                exam.MonthId = classRoomExamDTO.MonthId;
                exam.Rate = classRoomExamDTO.Rate;
                exam.SupervisorId = classRoomExamDTO.SupervisorId;
                exam.WeekDay = classRoomExamDTO.WeekDay;

                var updated = classRoomExamCommandService.Update(exam);

                if(updated)
                    return Ok(await classRoomExamQueryService.GetExamByIdAsync(exam.Id));

                return BadRequest("Error in updating exam");

            }
            catch (Exception ex)
            {
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
