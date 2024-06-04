using Mdaresna.Repository.IServices.AdminManagement.Command;
using Mdaresna.Repository.IServices.AdminManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.AdminManagement
{
    [Route("Language")]
    public class LanguageController : Controller
    {
        private readonly ILanguageQueryService languageQueryService;
        private readonly ILanguageCommandService languageCommandService;

        public LanguageController(ILanguageQueryService languageQueryService,
                                ILanguageCommandService languageCommandService)
        {
            this.languageQueryService = languageQueryService;
            this.languageCommandService = languageCommandService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> Index()
        {
            var res = await languageQueryService.GetAllAsync();
            return Ok(res);
        }
    }
}
