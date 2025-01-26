using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.SymbolStore;

namespace Mdaresna.Controllers.SchoolManagement.StudentManagement
{
    [Route("Student")]
    public class StudentController : Controller
    {
        private readonly IStudentCommandService studentCommandService;
        private readonly IStudentQueryService studentQueryService;

        public StudentController(IStudentCommandService studentCommandService,
                                 IStudentQueryService studentQueryService)
        {
            this.studentCommandService = studentCommandService;
            this.studentQueryService = studentQueryService;
        }

        [HttpPost("GetStudent")]
        public async Task<IActionResult> GetStudentById([FromBody] StudentIdDTO studentIdDTO)
        {
            try
            {
                //var result = await studentQueryService.GetByIdAsync(studentIdDTO.StudentId);
                var result = await studentQueryService.GetStudentByIdAsync(studentIdDTO.StudentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetStudentByCode")]
        public async Task<IActionResult> GetStudentByCode([FromBody] StudentCodeDTO studentCodeDTO)
        {
            try
            {
                //var result = await studentQueryService.GetByIdAsync(studentIdDTO.StudentId);
                var result = await studentQueryService.GetStudentByCodeAsync(studentCodeDTO.StudentCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetClassRoomStudents")]
        public async Task<IActionResult> GetClassRoomStudents([FromBody] SchoolIdClassRoomIdDTO schoolIdClassRoomIdDTO)
        {
            try
            {
                var result = await studentQueryService.GetStudentsBySchoolIdAndClassRoomIdAsync(schoolIdClassRoomIdDTO.SchoolId,
                                                                                          schoolIdClassRoomIdDTO.ClassRoomId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetSchoolStudents")]
        public async Task<IActionResult> GetSchoolStudents([FromBody] GetSchoolStudentsDTO dTO)
        {
            try
            {
                var result = await studentQueryService.GetStudentsBySchoolIdAsync(dTO.SchoolId, dTO.StudentCode, dTO.StudentName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddStudent")]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDTO studentDTO)
        {
            try
            {
                var studentCode = await GenerateCode(studentDTO.SchoolId);
                var student = new Student
                {
                    Code = studentCode,
                    FirstName = studentDTO.FirstName,
                    MiddelName = studentDTO.MidelName,
                    LastName = studentDTO.LastName,
                    ImageUrl = studentDTO.ImageUrl,
                    BirthDate = studentDTO.BirthDate,
                    ClassRoomId = studentDTO.ClassRoomId,
                    SchoolId = studentDTO.SchoolId,
                    IsPayed = studentDTO.IsPayed,
                    Active = studentDTO.Active
                };

                var added = studentCommandService.Create(student);

                if (added)
                    return Ok(student);

                return BadRequest("Error in adding student");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpPost("UpdateStudent")]
        public async Task<IActionResult> UpdateStudent([FromBody] UpdateStudentDTO studentDTO)
        {
            try
            {
                var student = await studentQueryService.GetByIdAsync(studentDTO.Id);

                if (student == null)
                    return BadRequest("Can't update student");


                student.FirstName = studentDTO.FirstName;
                student.MiddelName = studentDTO.MidelName;
                student.LastName = studentDTO.LastName;
                //student.ImageUrl = studentDTO.ImageUrl;
                student.BirthDate = studentDTO.BirthDate;
                student.ClassRoomId = studentDTO.ClassRoomId;
                student.SchoolId = studentDTO.SchoolId;
                student.IsPayed = studentDTO.IsPayed;
                student.Active = studentDTO.Active;

                var updated = studentCommandService.Update(student);

                if (updated)
                    return Ok(student);

                return BadRequest("Error in updating student");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("PayStudent")]
        public async Task<IActionResult> PayStudent([FromBody] StudentIdDTO studentIdDTO)
        {
            try
            {
                var student = await studentQueryService.GetByIdAsync(studentIdDTO.StudentId);

                if (student == null)
                    return BadRequest("Can't update student");

                var payed = await studentCommandService.Pay(student);


                return payed ? Ok("Student Payed") : BadRequest("Error in payment");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


        private async Task<string> GenerateCode(Guid schoolId)
        {
            var result = "ST";
            var lastcode = await studentQueryService.GetMaxStudebtCodeAsync(schoolId);
            if (lastcode == null)
                result += "0000000001";
            else
            {
                var code = int.Parse(lastcode.Replace(result, string.Empty));
                code++;
                result += code.ToString("D10");
            }

            return result;
        }





    }
}
