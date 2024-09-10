using Mdaresna.Doamin.DTOs.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.ClassRoomManagementDTO;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Mdaresna.Controllers.SchoolManagement.ClassRoomManagement
{
    [Route("Exams")]
    public class ClassRoomExamController : Controller
    {
        private readonly IClassRoomExamQueryService classRoomExamQueryService;
        private readonly IClassRoomExamCommandService classRoomExamCommandService;

        public ClassRoomExamController(IClassRoomExamQueryService classRoomExamQueryService,
                                       IClassRoomExamCommandService classRoomExamCommandService)
        {
            this.classRoomExamQueryService = classRoomExamQueryService;
            this.classRoomExamCommandService = classRoomExamCommandService;
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
                    return Ok(await classRoomExamQueryService.GetExamByIdAsync (exam.Id));

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












    }
}
