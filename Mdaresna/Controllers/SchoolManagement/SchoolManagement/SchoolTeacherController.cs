using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.Repository.IServices.IdentityManagement.Command;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
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
        private readonly IUserRoleCommandService userRoleCommandService;
        private readonly IUserRoleQueryService userRoleQueryService;

        public SchoolTeacherController(ISchoolTeacherCommandService schoolTeacherCommandService,
                                       ISchoolTeacherQueryService schoolTeacherQueryService,
                                       IUserRoleCommandService userRoleCommandService,
                                       IUserRoleQueryService userRoleQueryService)
        {
            this.schoolTeacherCommandService = schoolTeacherCommandService;
            this.schoolTeacherQueryService = schoolTeacherQueryService;
            this.userRoleCommandService = userRoleCommandService;
            this.userRoleQueryService = userRoleQueryService;
        }

        [HttpPost("AddSchoolTeacher")]
        public async Task<IActionResult> AddSchoolTeacher([FromBody] SchoolIdTeacherIdDTO schoolTeacher)
        {
            try
            {
                var teacher = await schoolTeacherQueryService.GetSchoolTeacherByIdAsync(schoolTeacher.SchoolId, schoolTeacher.TeacherId);

                if (teacher != null)
                    return Conflict("Teacher already assignd to school");
                
                var sTeacher = new SchoolTeacher
                {
                    SchoolId = schoolTeacher.SchoolId,
                    TeacherId = schoolTeacher.TeacherId
                };
                var added = schoolTeacherCommandService.Create(sTeacher);
                if (added)
                {
                    var userrole = new UserRole
                    {
                        RoleId = Guid.Parse("10620C5F-37FE-4D18-996F-915ECE8893F1"),
                        UserId = sTeacher.TeacherId,
                        SchoolId = schoolTeacher.SchoolId
                    };
                    var teacherRoleAssignd = await userRoleQueryService.CheckRoleExist(userrole);
                    if (!teacherRoleAssignd)
                    {
                        var roleadded = userRoleCommandService.Create(userrole);
                        if (!roleadded)
                            return BadRequest("Error in adding teacher role");
                    }
                    return Ok(schoolTeacher);
                }
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
