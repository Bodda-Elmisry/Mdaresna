using Mdaresna.DTOs.SettingsManagementDTO;
using Mdaresna.Repository.IServices.SettingsManagement.Query;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Mdaresna.Controllers.SettingsManagement
{
    [Route("SMSLog")]
    public class SMSLogController : Controller
    {
        private readonly ISMSLogQueryService smsLogQueryService;

        public SMSLogController(ISMSLogQueryService smsLogQueryService)
        {
            this.smsLogQueryService = smsLogQueryService;
        }

        [HttpPost("GetLogs")]
        public async Task<IActionResult> GetLogs([FromBody] GetSMSLogsDTO dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("Request cannot be null");
                }

                var logs = await smsLogQueryService.GetLogsList(dto.FromDate,
                                                                dto.ToDate,
                                                                dto.PhoneNumber,
                                                                dto.SMSProviderId,
                                                                dto.IsSuccess,
                                                                dto.Message,
                                                                dto.Response,
                                                                dto.PageNumber);

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
