using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.SchoolManagement
{
    [Route("SchoolYear")]
    public class SchoolYearController : Controller
    {
        private readonly ISchoolYearQueryService schoolYearQueryService;
        private readonly ISchoolYearCommandService schoolYearCommandService;

        public SchoolYearController(ISchoolYearQueryService schoolYearQueryService,
                                    ISchoolYearCommandService schoolYearCommandService)
        {
            this.schoolYearQueryService = schoolYearQueryService;
            this.schoolYearCommandService = schoolYearCommandService;
        }

        [HttpPost("GetCurrentYear")]
        public async Task<IActionResult> GetCurrentYear([FromBody] SchoolIdDTO schoolIdDTO)
        {
            try
            {
                var year = await schoolYearQueryService.GetCurrentYearAsync(schoolIdDTO.SchoolId);
                if(year == null)
                {
                    return NotFound("There is no current year");
                }
                return Ok(year);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetSchoolYears")]
        public async Task<IActionResult> GetSchoolYears([FromBody] SchoolIdDTO schoolId)
        {
            try
            {
                var years = await schoolYearQueryService.GetSchoolYearsAsync(schoolId.SchoolId);
                
                return Ok(years);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetSchoolYearById")]
        public async Task<IActionResult> GetShoolYear([FromBody] YearIdDTO yearId)
        {
            try
            {
                var year = await schoolYearQueryService.GetByIdAsync(yearId.SchoolYearId);

                if (year == null)
                    return NotFound("There is no year");

                var result = new SchoolYearResultDTO
                {
                    Id = year.Id,
                    Name = year.Name,
                    Active = year.IsActive,
                    Completed = year.Compleated,
                    Description = year.Description,
                    SchoolId = year.SchoolId
                };

                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CompleteSchoolYear")]
        public async Task<IActionResult> CompleteShoolYear([FromBody] YearIdDTO yearId)
        {
            try
            {
                var year = await schoolYearQueryService.GetByIdAsync(yearId.SchoolYearId);

                if (year == null)
                    return NotFound("There is no year to complete");

                //var completedYear = new SchoolYear
                //{
                //    Id = year.Id,
                //    Name = year.Name,
                //    SchoolId = year.SchoolId,
                //    Description = string.IsNullOrEmpty(year.Description) ? "Year completed in " + DateTime.Now.ToString() : year.Description + ", Year completed in " + DateTime.Now.ToString(),
                //    Compleated = true,
                //    IsActive = false

                //};

                year.Description = string.IsNullOrEmpty(year.Description) ? "Year completed in " + DateTime.Now.ToString() : year.Description + ", Year completed in " + DateTime.Now.ToString();
                year.Compleated = true;
                year.IsActive = false;

                var updated = schoolYearCommandService.Update(year);

                if (updated)
                    return Ok(year);
                
                return BadRequest("Error in Complete year");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ActivateSchoolYear")]
        public async Task<IActionResult> ActivateSchoolYear([FromBody] YearIdDTO yearId)
        {
            try
            {
                var year = await schoolYearQueryService.GetByIdAsync(yearId.SchoolYearId);

                if (year == null)
                    return NotFound("There is no year to Activate");

                //var updatedYear = new SchoolYear
                //{
                //    Id = year.Id,
                //    Name = year.Name,
                //    SchoolId = year.SchoolId,
                //    Description = year.Description,
                //    Compleated = year.Compleated,
                //    IsActive = true

                //};

                year.IsActive = true;

                var updated = schoolYearCommandService.Update(year);

                if (updated)
                    return Ok(year);

                return BadRequest("Error in Activate year");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("DeactivateSchoolYear")]
        public async Task<IActionResult> DeactivateSchoolYear([FromBody] YearIdDTO yearId)
        {
            try
            {
                var year = await schoolYearQueryService.GetByIdAsync(yearId.SchoolYearId);

                if (year == null)
                    return NotFound("There is no year to deactivate");

                //var updatedYear = new SchoolYear
                //{
                //    Id = year.Id,
                //    Name = year.Name,
                //    SchoolId = year.SchoolId,
                //    Description = year.Description,
                //    Compleated = year.Compleated,
                //    IsActive = false

                //};

                year.IsActive = false;

                var updated = schoolYearCommandService.Update(year);

                if (updated)
                    return Ok(year);

                return BadRequest("Error in deactivate year");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreateSchoolYear")]
        public IActionResult CreateSchoolYear([FromBody] CreateSchoolYearDTO schoolYearDTO)
        {
            try
            {
                var schoolYear = new SchoolYear
                {
                    Name = schoolYearDTO.Name,
                    Description = schoolYearDTO.Description,
                    Compleated = schoolYearDTO.Completed,
                    IsActive = schoolYearDTO.Active,
                    SchoolId = schoolYearDTO.SchoolId

                };

                var created = schoolYearCommandService.Create(schoolYear);

                if (created)
                    return Ok(schoolYear);

                return BadRequest("Error in ading year");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateSchoolYear")]
        public async Task<IActionResult> UpdateSchoolYear([FromBody] UpdateSchoolYearDTO schoolYearDTO)
        {
            try
            {
                var year = await schoolYearQueryService.GetByIdAsync(schoolYearDTO.Id);

                if (year == null)
                    return NotFound("Can't update school year");

                //var updatedYear = new SchoolYear
                //{
                //    Id = schoolYearDTO.Id,
                //    Name = schoolYearDTO.Name,
                //    SchoolId = schoolYearDTO.SchoolId,
                //    Description = schoolYearDTO.Description,
                //    Compleated = schoolYearDTO.Completed,
                //    IsActive = schoolYearDTO.Active

                //};

                year.Name = schoolYearDTO.Name;
                year.Description = schoolYearDTO.Description;
                year.IsActive = schoolYearDTO.Active;
                year.Compleated = schoolYearDTO.Completed;

                var updated = schoolYearCommandService.Update(year);

                if (updated)
                    return Ok(year);

                return BadRequest("Error in update year");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }












    }
}
