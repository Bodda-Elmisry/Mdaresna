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

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await schoolContactTypeQueryService.GetAllAsync();
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
                    IconUrl = schoolContactTypeDTO.IconeURL
                };
                var added = schoolContactTypeCommandService.Create(type);
                if(added)
                    return Ok(type);

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
                type.IconUrl = updateSchoolContactTypeDTO.IconeURL;

                var updated = schoolContactTypeCommandService.Update(type);
                if(updated) 
                    return Ok(type);

                return BadRequest("Error in update type");

            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }






    }
}
