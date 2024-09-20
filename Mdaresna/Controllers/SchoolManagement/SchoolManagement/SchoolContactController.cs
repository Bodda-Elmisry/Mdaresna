using Mdaresna.Doamin.Helpers;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.SchoolManagement
{
    [Route("SchoolContact")]
    public class SchoolContactController : Controller
    {
        private readonly ISchoolContactCommandService schoolContactCommandService;
        private readonly ISchoolContactQueryService schoolContactQueryService;

        public SchoolContactController(ISchoolContactCommandService schoolContactCommandService,
                                       ISchoolContactQueryService schoolContactQueryService)
        {
            this.schoolContactCommandService = schoolContactCommandService;
            this.schoolContactQueryService = schoolContactQueryService;
        }

        private string? GetTypeIconeURL(string? url)
        {
            return !string.IsNullOrEmpty(url) ? $"{SettingsHelper.GetAppUrl()}/{url.Replace("\\", "/")}" : string.Empty;
        }


        [HttpPost("GetSchoolContacts")]
        public async Task<IActionResult> GetSchoolContacts([FromBody] SchoolIdDTO schoolIdDTO)
        {
            try
            {
                var result = await schoolContactQueryService.GetSchoolContacts(schoolIdDTO.SchoolId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddContact")]
        public async Task<IActionResult> CreateContact([FromBody]CreateSchoolContactDTO createSchoolContactDTO)
        {
            try
            {
                var contact = new SchoolContact
                {
                    Value = createSchoolContactDTO.Value,
                    ContactTypeId = createSchoolContactDTO.SchoolContactTypeId,
                    SchoolId = createSchoolContactDTO.SchoolId
                };

                var added = schoolContactCommandService.Create(contact);
                
                if(added)
                    return Ok(await schoolContactQueryService.GetSchoolContactById(contact.Id));

                return BadRequest("Error in adding Contact");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateContact")]
        public async Task<IActionResult> UpdateContact([FromBody] UpdateSchoolContactDTO updateSchoolContactDTO)
        {
            try
            {
                var contact = await schoolContactQueryService.GetByIdAsync(updateSchoolContactDTO.Id);

                if (contact == null)
                    return BadRequest("Can't update contact");

                contact.Value = updateSchoolContactDTO.Value;
                contact.ContactTypeId = updateSchoolContactDTO.SchoolContactTypeId;

                var updated = schoolContactCommandService.Update(contact);

                if(updated) 
                    return Ok(await schoolContactQueryService.GetSchoolContactById(contact.Id));

                return BadRequest("Rttot in update contact");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }













    }
}
