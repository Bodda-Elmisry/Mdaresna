using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.StudentManagement
{
    [Route("ClassRoomStudentExam")]
    public class ClassRoomStudentExamController : Controller
    {
        private readonly IClassRoomStudentExamCommandService classRoomStudentExamCommandService;
        private readonly IClassRoomStudentExamQueryService classRoomStudentExamQueryService;
        private readonly IClassRoomExamCommandService classRoomExamCommandService;

        public ClassRoomStudentExamController(IClassRoomStudentExamCommandService classRoomStudentExamCommandService,
                                              IClassRoomStudentExamQueryService classRoomStudentExamQueryService,
                                              IClassRoomExamCommandService classRoomExamCommandService)
        {
            this.classRoomStudentExamCommandService = classRoomStudentExamCommandService;
            this.classRoomStudentExamQueryService = classRoomStudentExamQueryService;
            this.classRoomExamCommandService = classRoomExamCommandService;
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

        [HttpPost("UpdateStudentExam")]
        public async Task<IActionResult> UpdateStudentExam([FromBody] UpdateClassRoomStudentExamDTO dto)
        {
            try
            {
                var exam = await classRoomStudentExamQueryService.GetClassRoomStudentExamAsync(dto.StudentId, dto.ExamId);

                if (exam == null)
                    return BadRequest("There is no exam student to update");

                exam.TotalResult = dto.TotalResult;
                exam.IsAttend = dto.IsAttend;

                var updated = classRoomStudentExamCommandService.Update(exam);

                if (!updated) 
                    return BadRequest("Error in updating student exam");

                return Ok(await classRoomStudentExamQueryService.GetClassRoomStudentExamViewAsync(dto.StudentId, dto.ExamId));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




    }
}
