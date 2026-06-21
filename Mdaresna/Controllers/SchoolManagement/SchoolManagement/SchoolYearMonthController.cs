using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

using Microsoft.AspNetCore.Authorization;

namespace Mdaresna.Controllers.SchoolManagement.SchoolManagement
{
    [Route("SchoolYearMonth")]
    [Authorize]
    public class SchoolYearMonthController : Controller
    {
        private readonly ISchoolYearMonthQueryService schoolYearMonthQueryService;
        private readonly ISchoolYearMonthCommandService schoolYearMonthCommandService;
        private readonly Mdaresna.Repository.IServices.SettingsManagement.Command.IReportQueueCommandService reportQueueCommandService;

        public SchoolYearMonthController(ISchoolYearMonthQueryService schoolYearMonthQueryService,
                                         ISchoolYearMonthCommandService schoolYearMonthCommandService,
                                         Mdaresna.Repository.IServices.SettingsManagement.Command.IReportQueueCommandService reportQueueCommandService)
        {
            this.schoolYearMonthQueryService = schoolYearMonthQueryService;
            this.schoolYearMonthCommandService = schoolYearMonthCommandService;
            this.reportQueueCommandService = reportQueueCommandService;
        }

        [HttpPost("GetYearMonth")]
        public async Task<IActionResult> GetSchoolYearMonth([FromBody] MonthIdDTO monthId)
        {
            try
            {
                var result =  await schoolYearMonthQueryService.GetYearMonthAsync(monthId.MonthId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetYearMonths")]
        public async Task<IActionResult> GetSchoolYearMonths([FromBody] GetSchoolYearMonthsDTO getSchoolYearMonths)
        {
            try
            {
                var result = await schoolYearMonthQueryService.GetYearMonthesAsync(getSchoolYearMonths.SchoolYearId,
                                                                                   getSchoolYearMonths.IsActive,
                                                                                   getSchoolYearMonths.Name);
                return Ok(result);
                     
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetMonthsWithoutReports")]
        public async Task<IActionResult> GetMonthsWithoutReports([FromBody] YearIdDTO yearId)
        {
            try
            {
                var result = await schoolYearMonthQueryService.GetMonthsWithoutReportsAsync(yearId.SchoolYearId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("RequestMonthReport")]
        public async Task<IActionResult> RequestMonthReport([FromBody] Mdaresna.Doamin.DTOs.SettingsManagement.RequestMonthReportCommandDTO command)
        {
            try
            {
                var result = await reportQueueCommandService.RequestMonthReportAsync(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("PublishReportQueue")]
        public async Task<IActionResult> PublishReportQueue(
            [FromBody] Mdaresna.Doamin.DTOs.SettingsManagement.PublishReportQueueCommandDTO command,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await reportQueueCommandService.PublishReportQueueAsync(command, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("FailReportQueue")]
        public async Task<IActionResult> FailReportQueue(
            [FromBody] Mdaresna.Doamin.DTOs.SettingsManagement.FailReportQueueCommandDTO command,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await reportQueueCommandService.FailReportQueueAsync(command, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreateYearMonth")]
        public IActionResult CreateSchoolYearMonths([FromBody] CreateSchoolYearMonthDTO SchoolYearMonth)
        {
            try
            {
                var month = new SchoolYearMonth
                {
                    Name = SchoolYearMonth.Name,
                    Description = SchoolYearMonth.Description,
                    IsActive = SchoolYearMonth.IsActive,
                    YearId = SchoolYearMonth.YearId
                };

                var created = schoolYearMonthCommandService.Create(month);

                if(created)
                    return Ok(new SchoolYearMonthResultDTO
                    {
                        Id = month.Id,
                        Name = month.Name,
                        Description = month.Description,
                        IsActive = month.IsActive,
                        YearId = month.YearId
                    });

                return BadRequest("Eror in create Month");


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateYearMonth")]
        public async Task<IActionResult> UpdateSchoolYearMonths([FromBody] UpdateSchoolYearMonthDTO schoolYearMonth)
        {
            try
            {
                var month = await schoolYearMonthQueryService.GetByIdAsync(schoolYearMonth.Id);
                if (month == null) 
                    return BadRequest("Can't fiend Month");

                month.Name = schoolYearMonth.Name;
                month.Description = schoolYearMonth.Description;
                month.IsActive = schoolYearMonth.IsActive;
                month.YearId = schoolYearMonth.YearId;

                var updated = schoolYearMonthCommandService.Update(month);

                if (updated)
                    return Ok(new SchoolYearMonthResultDTO
                    {
                        Id = month.Id,
                        Name = month.Name,
                        Description = month.Description,
                        IsActive = month.IsActive,
                        YearId = month.YearId
                    });

                return BadRequest("Error in update month");


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ActivateYearMonth")]
        public async Task<IActionResult> ActivateSchoolYearMonths([FromBody] MonthIdDTO monthId)
        {
            try
            {
                var month = await schoolYearMonthQueryService.GetByIdAsync(monthId.MonthId);
                if (month == null)
                    return BadRequest("Can't fiend Month");

                month.IsActive = true;
                

                var updated = schoolYearMonthCommandService.Update(month);

                if (updated)
                    return Ok(new SchoolYearMonthResultDTO
                    {
                        Id = month.Id,
                        Name = month.Name,
                        Description = month.Description,
                        IsActive = month.IsActive,
                        YearId = month.YearId
                    });

                return BadRequest("Error in activate month");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("DeactivateYearMonth")]
        public async Task<IActionResult> DeactivateSchoolYearMonths([FromBody] MonthIdDTO monthId)
        {
            try
            {
                var month = await schoolYearMonthQueryService.GetByIdAsync(monthId.MonthId);
                if (month == null)
                    return BadRequest("Can't fiend Month");

                month.IsActive = false;


                var updated = schoolYearMonthCommandService.Update(month);

                if (updated)
                    return Ok(new SchoolYearMonthResultDTO
                    {
                        Id = month.Id,
                        Name = month.Name,
                        Description = month.Description,
                        IsActive = month.IsActive,
                        YearId = month.YearId
                    });

                return BadRequest("Error in deactivate month");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteYearMonth")]
        public async Task<IActionResult> SoftDeleteSchoolYearMonths([FromBody] MonthIdDTO monthId)
        {
            try
            {
                var month = await schoolYearMonthQueryService.GetByIdAsync(monthId.MonthId);
                if (month == null)
                    return BadRequest("Can't fiend Month");

                month.Deleted = true;
                var deleted = schoolYearMonthCommandService.Update(month);
                return deleted ? Ok("Month Deleted") : BadRequest("Error in delete month");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }










            }
}
