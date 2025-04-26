using Mdaresna.Doamin.Models.AdminManagement;
using Mdaresna.DTOs.AdminManagementDTO;
using Mdaresna.DTOs.Common;
using Mdaresna.Repository.IServices.AdminManagement.Command;
using Mdaresna.Repository.IServices.AdminManagement.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

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

        [HttpPost("AddLanguage")]
        public IActionResult CreateNewLanguage([FromBody] CreateLanguageDTO languageDTO)
        {
            try
            {
                var lang = new Language
                {
                    Name = languageDTO.Name,
                    Description = languageDTO.Description
                };

                var result = languageCommandService.Create(lang);
                if (result)
                    return Ok(lang);
                else
                    return BadRequest("Missing Data");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateLanguage")]
        public async Task<IActionResult> UpdateLanguage([FromBody] UpdateCreateLanguageDTO language)
        {
            try
            {

                var lang = await languageQueryService.GetByIdAsync(language.Id);
                if (lang == null)
                    return BadRequest("Can't fiend Language");
                lang.Name = language.Name;
                lang.Description = language.Description;
                var result = languageCommandService.Update(lang);
                if (result)
                    return Ok(language);
                else
                    return BadRequest("Missing Data");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteLanguage")]
        public async Task<IActionResult> SoftDeleteLanguage([FromBody] LanguageIdDTO dto)
        {
            try
            {
                var language = await languageQueryService.GetByIdAsync(dto.LanguageId);

                if (language == null)
                    return BadRequest("Language Not Found");

                language.Deleted = true;

                var result = languageCommandService.Update(language);

                return result ? Ok("Language Deleted") : BadRequest("Error in delete language");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }



        }
    }
}
