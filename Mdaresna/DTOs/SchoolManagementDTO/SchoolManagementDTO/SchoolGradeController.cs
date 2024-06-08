using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO
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
        public IActionResult EditSchoolGrade([FromBody] SchoolGrade schoolGrade)
        {
            try
            {
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



    }
}
