using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Mdaresna.Controllers.Common
{
    [Route("API/SystemSettings")]
    public class SystemSettingsController : Controller
    {
        private readonly IConfiguration _configuration;

        public SystemSettingsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("AppVersion")]
        public IActionResult GetAppVersion()
        {
            var section = _configuration.GetSection("AppVersionSettings");
            return Ok(new
            {
                AndroidLatestVersion = section["AndroidLatestVersion"] ?? "1.0.2",
                AndroidMinRequiredVersion = section["AndroidMinRequiredVersion"] ?? "1.0.0",
                iOSLatestVersion = section["iOSLatestVersion"] ?? "1.0.2",
                iOSMinRequiredVersion = section["iOSMinRequiredVersion"] ?? "1.0.0"
            });
        }
    }
}
