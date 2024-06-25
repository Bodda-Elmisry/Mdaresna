using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.SchoolManagement
{
    [Route("SchoolTeacher")]
    public class SchoolTeacherController : Controller
    {
        private readonly ISchoolTeacherCommandService schoolTeacherCommandService;
        private readonly ISchoolTeacherQueryService schoolTeacherQueryService;

        public SchoolTeacherController(ISchoolTeacherCommandService schoolTeacherCommandService,
                                       ISchoolTeacherQueryService schoolTeacherQueryService)
        {
            this.schoolTeacherCommandService = schoolTeacherCommandService;
            this.schoolTeacherQueryService = schoolTeacherQueryService;
        }

        [HttpPost("AddSchoolTeacher")]
        public IActionResult AddSchoolTeacher([FromBody] SchoolIdTeacherIdDTO schoolTeacher)
        {
            try
            {
                var sTeacher = new SchoolTeacher
                {
                    SchoolId = schoolTeacher.SchoolId,
                    TeacherId = schoolTeacher.TeacherId
                };
                var added = schoolTeacherCommandService.Create(sTeacher);
                if (added)
                    return Ok(schoolTeacher);
                return BadRequest("Can't add teacher to school");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateSchoolTeacher")]
        public async Task<IActionResult> UpdateSchoolTeacher([FromBody] SchoolIdTeacherIdDTO schoolTeacher)
        {
            try
            {
                var sTeacher = await schoolTeacherQueryService.GetSchoolTeacherByIdAsync(schoolTeacher.SchoolId, schoolTeacher.TeacherId);

                if (sTeacher == null)
                    return BadRequest("Can't update teacher");

                

                var updated = schoolTeacherCommandService.Update(sTeacher);

                if(updated)
                    return Ok(sTeacher);

                return BadRequest("Error in update school teacher");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("RemoveTeacherFromSchool")]
        public async Task<IActionResult> RemoveSchoolTeacher([FromBody] SchoolIdTeacherIdDTO schoolTeacher)
        {
            try
            {
                var sTeacher = new SchoolTeacher
                {
                    SchoolId = schoolTeacher.SchoolId,
                    TeacherId = schoolTeacher.TeacherId
                };

                var deleted = await schoolTeacherCommandService.DeleteAsync(sTeacher);
                if (deleted)
                    return Ok("Teacher removed from school");

                return BadRequest("Error in removing");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetSchoolTeachers")]
        public async Task<IActionResult> GetSchoolTeachers([FromBody] SchoolIdDTO schoolIdDTO)
        {
            try
            {
                var teachers = await schoolTeacherQueryService.GetSchoolTeachersAsync(schoolIdDTO.SchoolId);
                return Ok(teachers);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetTeacherSchools")]
        public async Task<IActionResult> GetTeacherSchools([FromBody] TeacherIdDTO teacherIdDTO)
        {
            try
            {
                var schools = await schoolTeacherQueryService.GetTeacherSchoolsAsync(teacherIdDTO.TeacherId);
                return Ok(schools);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }











    }
}
