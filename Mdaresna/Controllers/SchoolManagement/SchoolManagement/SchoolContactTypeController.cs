using Mdaresna.Doamin.Helpers;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.SchoolManagement
{
    [Route("SchoolContactType")]
    public class SchoolContactTypeController : Controller
    {
        private readonly ISchoolContactTypeCommandService schoolContactTypeCommandService;
        private readonly ISchoolContactTypeQueryService schoolContactTypeQueryService;

        public SchoolContactTypeController(ISchoolContactTypeCommandService schoolContactTypeCommandService,
                                           ISchoolContactTypeQueryService schoolContactTypeQueryService)
        {
            this.schoolContactTypeCommandService = schoolContactTypeCommandService;
            this.schoolContactTypeQueryService = schoolContactTypeQueryService;
        }

        private string? GetTypeIconeURL(string? url)
        {
            return !string.IsNullOrEmpty(url) ? $"{SettingsHelper.GetAppUrl()}/{url.Replace("\\", "/")}" : string.Empty;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await schoolContactTypeQueryService.GetAllAsync();
                var schools = result.Select(t => new SchoolContactType
                {
                    Id = t.Id,
                    Name = t.Name,
                    IconUrl = GetTypeIconeURL(t.IconUrl),
                    Description = t.Description,
                    CreateDate = t.CreateDate,
                    LastModifyDate = t.LastModifyDate
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetContactType")]
        public async Task<IActionResult> GetCnatactTypeByID([FromBody] SchoolContactTypeIdDTO schoolContactTypeIdDTO)
        {
            try
            {
                var result = await schoolContactTypeQueryService.GetByIdAsync(schoolContactTypeIdDTO.SchoolContactTypeId);
                result.IconUrl = GetTypeIconeURL(result.IconUrl);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddContactType")]
        public IActionResult AddContactType([FromBody] CreateSchoolContactTypeDTO schoolContactTypeDTO)
        {
            try
            {
                var type = new SchoolContactType
                {
                    Name = schoolContactTypeDTO.Name,
                    Description = schoolContactTypeDTO.Description,
                };
                var added = schoolContactTypeCommandService.Create(type);
                if (added)
                {
                    type.IconUrl = GetTypeIconeURL(type.IconUrl);
                    return Ok(type);
                }

                return BadRequest("Error in adding");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateContactType")]
        public async Task<IActionResult> UpdateContactType([FromBody] UpdateSchoolContactTypeDTO updateSchoolContactTypeDTO)
        {
            try
            {
                var type = await schoolContactTypeQueryService.GetByIdAsync(updateSchoolContactTypeDTO.Id);
                if (type == null)
                    return BadRequest("Can't Update Type");

                type.Name = updateSchoolContactTypeDTO.Name;
                type.Description = updateSchoolContactTypeDTO.Description;

                var updated = schoolContactTypeCommandService.Update(type);
                if (updated)
                {
                    type.IconUrl = GetTypeIconeURL(type.IconUrl);
                    return Ok(type);
                }

                return BadRequest("Error in update type");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteContactType")]
        public async Task<IActionResult> SoftDeleteContactType([FromBody] SchoolContactTypeIdDTO schoolContactTypeIdDTO)
        {
            try
            {
                var type = await schoolContactTypeQueryService.GetByIdAsync(schoolContactTypeIdDTO.SchoolContactTypeId);
                if (type == null)
                    return BadRequest("There is no type to delete");

                type.Deleted = true;

                var deleted = schoolContactTypeCommandService.Update(type);
                if (deleted)
                    return Ok("Contact type deleted");

                return BadRequest("Error in deleting type");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }






    }
}
