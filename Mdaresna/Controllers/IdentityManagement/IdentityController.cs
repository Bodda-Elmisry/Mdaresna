using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.IdentityManagement
{
    [Route("API/Identity")]
    public class IdentityController : Controller
    {
        public IActionResult Regidter()
        {
            return View();
        }
    }
}
