using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.SchoolManagement
{
    [Route("Grade")]
    public class SchoolGradeController : Controller
    {
        private readonly ISchoolGradeQueryService schoolGradeQueryService;
        private readonly ISchoolGradeCommandService schoolGradeCommandService;

        public SchoolGradeController(ISchoolGradeQueryService schoolGradeQueryService,
                                     ISchoolGradeCommandService schoolGradeCommandService)
        {
            this.schoolGradeQueryService = schoolGradeQueryService;
            this.schoolGradeCommandService = schoolGradeCommandService;
        }
        [HttpPost("GetSchoolGrades")]
        public async Task<IActionResult> GetSchoolGrades([FromBody] SchoolIdDTO schoolId)
        {
            try
            {
                var grades = await schoolGradeQueryService.GetSchoolGradesAsync(schoolId.SchoolId);
                return Ok(grades);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetSchoolGradeById")]
        public async Task<IActionResult> GetSchoolGradeById([FromBody] SchoolGradeIdDTO gradeId)
        {
            try
            {
                var grade = await schoolGradeQueryService.GetByIdAsync(gradeId.GradeId);
                return Ok(grade);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddGrade")]
        public IActionResult CreateSchoolGrade([FromBody] CreateSchoolGradeDTO schoolGrade)
        {
            try
            {
                var grade = new SchoolGrade
                {
                    Name = schoolGrade.Name,
                    Notes = schoolGrade.Notes,
                    SchoolId = schoolGrade.SchoolId
                };

                var created = schoolGradeCommandService.Create(grade);
                if (created)
                    return Ok(grade);
                else
                    return BadRequest("Error In Adding Grade");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("EditGrade")]
        public async Task<IActionResult> EditSchoolGrade([FromBody] UpdateSchoolGradeDTO schoolGradeDTO)
        {
            try
            {
                SchoolGrade schoolGrade = await schoolGradeQueryService.GetByIdAsync(schoolGradeDTO.Id);
                schoolGrade.Name = schoolGradeDTO.Name;
                schoolGrade.Notes = schoolGradeDTO.Notes;

                var Updated = schoolGradeCommandService.Update(schoolGrade);
                if (Updated)
                    return Ok(schoolGrade);
                else
                    return BadRequest("Error In Edit Grade");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteGrade")]
        public async Task<IActionResult> SoftDeleteGrade([FromBody] SchoolGradeIdDTO schoolGradeId)
        {
            try
            {
                var grade = await schoolGradeQueryService.GetByIdAsync(schoolGradeId.GradeId);
                if (grade == null)
                    return BadRequest("There is no grade to delete");

                grade.Deleted = true;

                var deleted = schoolGradeCommandService.Update(grade);
                if (deleted)
                    return Ok("Grade deleted");
                else
                    return BadRequest("Error In Deleting Grade");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



    }
}
