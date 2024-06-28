using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.SchoolManagement
{
    [Route("SchoolTeacherCourse")]
    public class SchoolTeacherCourseController : Controller
    {
        private readonly ISchoolTeacherCourseQueryService schoolTeacherCourseQueryService;
        private readonly ISchoolTeacherCourseCommandService schoolTeacherCourseCommandService;

        public SchoolTeacherCourseController(ISchoolTeacherCourseQueryService schoolTeacherCourseQueryService,
                                             ISchoolTeacherCourseCommandService schoolTeacherCourseCommandService)
        {
            this.schoolTeacherCourseQueryService = schoolTeacherCourseQueryService;
            this.schoolTeacherCourseCommandService = schoolTeacherCourseCommandService;
        }

        [HttpPost("GetInitialLists")]
        public async Task<IActionResult> GetInitialLists([FromBody] SchoolIdDTO schoolId)
        {
            try
            {
                var result = await schoolTeacherCourseQueryService.GetSchoolTeacherCourceInitialListsAsync(schoolId.SchoolId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetSchoolTeacherCoursesList")]
        public async Task<IActionResult> GetSchoolTeacherCoursesList([FromBody]SchoolIdTeacherIdDTO schoolIdTeacherId)
        {
            try
            {
                var courses = await schoolTeacherCourseQueryService.GetSchoolTeacherCourcesAsync(schoolIdTeacherId.SchoolId, schoolIdTeacherId.TeacherId);
                return Ok(courses);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetSchoolTeacherCourse")]
        public async Task<IActionResult> GetSchoolTeacherCourse([FromBody] SchoolIdTeacherIdCourseIdDTO schoolIdTeacherIdCourseId)
        {
            try
            {
                var courses = await schoolTeacherCourseQueryService.GetSchoolTeacherCourceAsync(schoolIdTeacherIdCourseId.SchoolId,
                                                                                                schoolIdTeacherIdCourseId.TeacherId,
                                                                                                schoolIdTeacherIdCourseId.CourseId);
                return Ok(courses);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddSchoolTeacherCourse")]
        public async Task<IActionResult> AddSchoolTeacherCourse([FromBody] SchoolIdTeacherIdCourseIdDTO schoolIdTeacherIdCourseId)
        {
            try
            {
                var course = new SchoolTeacherCourse
                {
                    SchoolId = schoolIdTeacherIdCourseId.SchoolId,
                    TeacherId = schoolIdTeacherIdCourseId.TeacherId,
                    CourseId = schoolIdTeacherIdCourseId.CourseId
                };

                var added = schoolTeacherCourseCommandService.Create(course);
                if (added)
                    return Ok(course);

                return BadRequest("Error in Adding course to teacher");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[HttpPost("UpdateSchoolTeacherCourse")]
        //public async Task<IActionResult> UpdateSchoolTeacherCourse([FromBody] SchoolIdTeacherIdCourseIdDTO schoolIdTeacherIdCourseId)
        //{
        //    try
        //    {
        //        var course = await schoolTeacherCourseQueryService.GetSchoolTeacherCourceAsync(schoolIdTeacherIdCourseId.SchoolId,
        //                                                                                        schoolIdTeacherIdCourseId.TeacherId,
        //                                                                                        schoolIdTeacherIdCourseId.CourseId);
        //        if (course == null)
        //            return BadRequest("Can't update teacher course");

        //        var updated = schoolTeacherCourseCommandService.Update(course);

        //        if (updated)
        //            return Ok(course);

        //        return BadRequest("Error in update teacher course");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        [HttpPost("RemoveSchoolTeacherCourse")]
        public async Task<IActionResult> DeleteSchoolTeacherCourse([FromBody] SchoolIdTeacherIdCourseIdDTO schoolIdTeacherIdCourseId)
        {
            try
            {
                var course = new SchoolTeacherCourse
                {
                    SchoolId = schoolIdTeacherIdCourseId.SchoolId,
                    TeacherId = schoolIdTeacherIdCourseId.TeacherId,
                    CourseId = schoolIdTeacherIdCourseId.CourseId
                };

                var deleted = await schoolTeacherCourseCommandService.DeleteAsync(course);

                if (deleted)
                    return Ok("Deleted");

                return BadRequest("Error in delete teacher course");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }







    }
}
