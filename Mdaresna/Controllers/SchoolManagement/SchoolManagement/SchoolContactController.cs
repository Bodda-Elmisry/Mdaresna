using Mdaresna.Doamin.Helpers;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace Mdaresna.Controllers.SchoolManagement.SchoolManagement
{
    [Route("SchoolContact")]
    public class SchoolContactController : Controller
    {
        private readonly ISchoolContactCommandService schoolContactCommandService;
        private readonly ISchoolContactQueryService schoolContactQueryService;
        private readonly ISchoolContactTypeQueryService schoolContactTypeQueryService;

        public SchoolContactController(ISchoolContactCommandService schoolContactCommandService,
                                       ISchoolContactQueryService schoolContactQueryService,
                                       ISchoolContactTypeQueryService schoolContactTypeQueryService)
        {
            this.schoolContactCommandService = schoolContactCommandService;
            this.schoolContactQueryService = schoolContactQueryService;
            this.schoolContactTypeQueryService = schoolContactTypeQueryService;
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
        [Authorize]
        public async Task<IActionResult> CreateContact([FromBody] CreateSchoolContactDTO createSchoolContactDTO)
        {
            try
            {
                var contactType = await schoolContactTypeQueryService.GetByIdAsync(
                    createSchoolContactDTO.SchoolContactTypeId);
                if (contactType == null)
                    return BadRequest("Can't find contact type.");

                if (!SchoolContactValueValidator.TryValidate(
                        contactType.ActionType,
                        createSchoolContactDTO.Value,
                        out var validationError))
                    return BadRequest(validationError);

                var contact = new SchoolContact
                {
                    Value = createSchoolContactDTO.Value.Trim(),
                    ContactTypeId = createSchoolContactDTO.SchoolContactTypeId,
                    SchoolId = createSchoolContactDTO.SchoolId
                };

                var added = schoolContactCommandService.Create(contact);

                if (added)
                    return Ok(await schoolContactQueryService.GetSchoolContactById(contact.Id));

                return BadRequest("Error in adding Contact");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateContact")]
        [Authorize]
        public async Task<IActionResult> UpdateContact([FromBody] UpdateSchoolContactDTO updateSchoolContactDTO)
        {
            try
            {
                var contact = await schoolContactQueryService.GetByIdAsync(updateSchoolContactDTO.Id);

                if (contact == null)
                    return BadRequest("Can't update contact");

                var contactType = await schoolContactTypeQueryService.GetByIdAsync(
                    updateSchoolContactDTO.SchoolContactTypeId);
                if (contactType == null)
                    return BadRequest("Can't find contact type.");

                if (!SchoolContactValueValidator.TryValidate(
                        contactType.ActionType,
                        updateSchoolContactDTO.Value,
                        out var validationError))
                    return BadRequest(validationError);

                contact.Value = updateSchoolContactDTO.Value.Trim();
                contact.ContactTypeId = updateSchoolContactDTO.SchoolContactTypeId;

                var updated = schoolContactCommandService.Update(contact);

                if (updated)
                    return Ok(await schoolContactQueryService.GetSchoolContactById(contact.Id));

                return BadRequest("Rttot in update contact");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("DeleteContact")]
        [Authorize]
        public async Task<IActionResult> DeleteSchoolContact([FromBody] SchoolContactIdDTO dto)
        {
            try
            {
                var contact = await schoolContactQueryService.GetByIdAsync(dto.SchoolContactId);
                if (contact == null)
                    return BadRequest("Can't find contact to delete");

                var deleted = await schoolContactCommandService.DeleteAsync(contact);

                return deleted ? Ok("Contact Deleted") : BadRequest("Error in deleting contact");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }













    }
}
