using Mdaresna.Doamin.DTOs.ReportingDTOs.StudentWeeklyReportDTOs;
using Mdaresna.Repository.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers
{
    [Route("Reporting")]
    [Authorize]
    public class ReportingController : Controller
    {
        private readonly IReportingService reportingService;

        public ReportingController(IReportingService reportingService)
        {
            this.reportingService = reportingService;
        }

        [HttpPost("GetStudentWeeklyReport")]
        public async Task<IActionResult> GetStudentWeeklyReport([FromBody] StudentWeeklyReportRequestDTO request)
        {
            var result = await reportingService.GetStudentWeeklyReport(request);

            return Ok(result);
        }
    }
}
