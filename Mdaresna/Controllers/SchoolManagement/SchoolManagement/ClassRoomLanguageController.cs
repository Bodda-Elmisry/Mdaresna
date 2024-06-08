using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.SchoolManagement
{
    [Route("SchoolLanguage")]
    public class ClassRoomLanguageController : Controller
    {
        private readonly IClassRoomLanguageCommandService classRoomLanguageCommandService;
        private readonly IClassRoomLanguageQueryService classRoomLanguageQueryService;

        public ClassRoomLanguageController(IClassRoomLanguageCommandService classRoomLanguageCommandService,
                                           IClassRoomLanguageQueryService classRoomLanguageQueryService)
        {
            this.classRoomLanguageCommandService = classRoomLanguageCommandService;
            this.classRoomLanguageQueryService = classRoomLanguageQueryService;
        }

        [HttpPost("GetLanguageSchools")]
        public async Task<IActionResult> GetLanguageSchools([FromBody] Guid languageId)
        {
            try
            {
                var result = await classRoomLanguageQueryService.GetLanguageSchools(languageId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetSchoolLanguages")]
        public async Task<IActionResult> GetSchoolLanguages([FromBody] SchoolIdDTO schoolId)
        {
            try
            {
                var result = await classRoomLanguageQueryService.GetSchoolLanguages(schoolId.SchoolId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AssignLanguage")]
        public IActionResult CreateNewSchoolLanguage([FromBody] CreateSchoolLanguageDTO classRoomLanguageDTO)
        {
            try
            {
                var classRoomLanguage = new ClassRoomLanguage
                {
                    SchoolId = classRoomLanguageDTO.SchoolId,
                    LanguageId = classRoomLanguageDTO.LanguageId
                };

                var added = classRoomLanguageCommandService.Create(classRoomLanguage);
                if (added)
                    return Ok(classRoomLanguage);
                else
                    return BadRequest("Error In Assign");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



    }
}
