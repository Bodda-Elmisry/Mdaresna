using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.SchoolManagement
{
    [Route("SchoolType")]
    public class SchoolTypeController : Controller
    {
        private readonly ISchoolTypeQueryService schoolTypeQueryService;

        public SchoolTypeController(ISchoolTypeQueryService schoolTypeQueryService)
        {
            this.schoolTypeQueryService = schoolTypeQueryService;
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var result = await schoolTypeQueryService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
