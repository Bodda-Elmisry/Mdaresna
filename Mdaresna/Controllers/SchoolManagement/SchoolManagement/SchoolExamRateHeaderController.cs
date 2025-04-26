using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.SchoolManagement
{
    [Route("ExamRateHeader")]
    public class SchoolExamRateHeaderController : Controller
    {
        private readonly ISchoolExamRateHeaderCommandService schoolExamRateHeaderCommandService;
        private readonly ISchoolExamRateHeaderQueryService schoolExamRateHeaderQueryService;

        public SchoolExamRateHeaderController(ISchoolExamRateHeaderCommandService schoolExamRateHeaderCommandService,
                                              ISchoolExamRateHeaderQueryService schoolExamRateHeaderQueryService)
        {
            this.schoolExamRateHeaderCommandService = schoolExamRateHeaderCommandService;
            this.schoolExamRateHeaderQueryService = schoolExamRateHeaderQueryService;
        }

        [HttpPost("GetRateHeader")]
        public async Task<IActionResult> GetExamRateHeader([FromBody] RateHeaderIdDTO rateHeaderId)
        {
            try
            {
                var header = await schoolExamRateHeaderQueryService.GetRateHeaderAsync(rateHeaderId.RateHeaderId);
                return Ok(header);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetRateHeadersList")]
        public async Task<IActionResult> GetExamRateHeaders([FromBody] GetSchoolExamRateHeadersDTO getSchoolExamRateHeaders)
        {
            try
            {
                var headers = await schoolExamRateHeaderQueryService.GetRateHeadersAsync(getSchoolExamRateHeaders.SchoolId,
                                                                                         getSchoolExamRateHeaders.Name,
                                                                                         getSchoolExamRateHeaders.Percentage);
                return Ok(headers);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreateRateHeadersList")]
        public IActionResult CreateExamRateHeaders([FromBody] CreateSchoolExamRateHeaderDTO schoolExamRateHeader)
        {
            try
            {
                var header = new SchoolExamRateHeader
                {
                    Name = schoolExamRateHeader.Name,
                    Note = schoolExamRateHeader.Notes,
                    Percentage = schoolExamRateHeader.Percentage,
                    SchoolId = schoolExamRateHeader.SchoolId
                };

                var created = schoolExamRateHeaderCommandService.Create(header);

                if (created)
                    return Ok(new SchoolExamRateHeaderResultDTO
                    {
                        Id = header.Id,
                        Name = header.Name,
                        Note = header.Note,
                        Percentage = header.Percentage,
                        SchoolId = header.SchoolId
                    });

                return BadRequest("Error in create rate header");


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateRateHeadersList")]
        public async Task<IActionResult> UpdateExamRateHeaders([FromBody] UpdateSchoolExamRateHeaderDTO schoolExamRateHeader)
        {
            try
            {
                var header = await schoolExamRateHeaderQueryService.GetByIdAsync(schoolExamRateHeader.Id);

                if (header == null)
                    return BadRequest("Can't fiend header");

                header.Name = schoolExamRateHeader.Name;
                header.Note = schoolExamRateHeader.Notes;
                header.Percentage = schoolExamRateHeader.Percentage;

                var update = schoolExamRateHeaderCommandService.Update(header);

                if (update)
                    return Ok(new SchoolExamRateHeaderResultDTO
                    {
                        Id = header.Id,
                        Name = header.Name,
                        Note = header.Note,
                        Percentage = header.Percentage,
                        SchoolId = header.SchoolId
                    });

                return BadRequest("Error in update rate header");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteRateHeader")]
        public async Task<IActionResult> SoftDeleteRateHeader([FromBody] RateHeaderIdDTO rateHeaderId)
        {
            try
            {
                var header = await schoolExamRateHeaderQueryService.GetByIdAsync(rateHeaderId.RateHeaderId);

                if (header == null)
                    return BadRequest("Can't find header");

                header.Deleted = true;

                var deleted = schoolExamRateHeaderCommandService.Update(header);

                if (deleted)
                    return Ok("Header deleted");

                return BadRequest("Error in delete header");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }











    }
}
