﻿using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.SchoolManagement
{
    [Route("SchoolCources")]
    public class SchoolCourseController : Controller
    {
        private readonly ISchoolCourseCommandService schoolCourseCommandService;
        private readonly ISchoolCourseQueryService schoolCourseQueryService;

        public SchoolCourseController(ISchoolCourseCommandService schoolCourseCommandService,
                                      ISchoolCourseQueryService schoolCourseQueryService)
        {
            this.schoolCourseCommandService = schoolCourseCommandService;
            this.schoolCourseQueryService = schoolCourseQueryService;
        }

        [HttpPost("GetSchoolCources")]
        public async Task<IActionResult> GetSchoolCources([FromBody] SchoolIdDTO schoolIdDTO)
        {
            try
            {
                var result = await schoolCourseQueryService.GetCoursesBySchoolIdAsync(schoolIdDTO.SchoolId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetSchoolCourceById")]
        public async Task<IActionResult> GetSchoolCourceById([FromBody] CourseIdDTO courseIdDTO)
        {
            try
            {
                var result = await schoolCourseQueryService.GetCourseIDAsync(courseIdDTO.CourseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetSchoolLanguageCources")]
        public async Task<IActionResult> GetSchoolLanguageCources([FromBody] SchoolIdLanguageIdDTO schoolIdLanguageIdDTO)
        {
            try
            {
                var result = await schoolCourseQueryService.GetCoursesBySchoolIdAndLanguageIDAsync(schoolIdLanguageIdDTO.SchoolId, schoolIdLanguageIdDTO.LanguageId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddCourse")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateSchoolCourseDTO createSchoolCourseDTO)
        {
            try
            {
                var course = new SchoolCourse
                {
                    Name = createSchoolCourseDTO.Name,
                    Description = createSchoolCourseDTO.Description,
                    SchoolId = createSchoolCourseDTO.SchoolId,
                    LanguageId = createSchoolCourseDTO.LanguageId
                };

                var added = schoolCourseCommandService.Create(course);
                if (added)
                    return Ok(await schoolCourseQueryService.GetCourseIDAsync(course.Id));

                return BadRequest("Error in adding course");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateCourse")]
        public async Task<IActionResult> UpdateCourse([FromBody] UpdateSchoolCourseDTO updateSchoolCourseDTO)
        {
            try
            {
                var course = await schoolCourseQueryService.GetByIdAsync(updateSchoolCourseDTO.Id);

                if (course == null)
                    return BadRequest("Can't update course");

                course.Name = updateSchoolCourseDTO.Name;
                course.Description = updateSchoolCourseDTO.Description;
                course.SchoolId = updateSchoolCourseDTO.SchoolId;
                course.LanguageId = updateSchoolCourseDTO.LanguageId;

                var updated = schoolCourseCommandService.Update(course);

                if (updated)
                    return Ok(await schoolCourseQueryService.GetCourseIDAsync(course.Id));

                return BadRequest("Error in updating course");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteSchoolCourse")]
        public async Task<IActionResult> SoftDeleteSchoolCourse([FromBody] CourseIdDTO courseIdDTO)
        {
            try
            {
                var course = await schoolCourseQueryService.GetByIdAsync(courseIdDTO.CourseId);

                if (course == null)
                    return BadRequest("There is no course to delete");

                course.Deleted = true;

                var deleted = schoolCourseCommandService.Update(course);

                
                return deleted ? Ok("Course deleted") : BadRequest("Error in deleting course");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }





    }
}
