using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.SchoolManagement
{
    [Route("SchoolType")]
    public class SchoolTypeController : Controller
    {
        private readonly ISchoolTypeQueryService schoolTypeQueryService;
        private readonly ISchoolTypeCommandService schoolTypeCommandService;

        public SchoolTypeController(ISchoolTypeQueryService schoolTypeQueryService,
                                    ISchoolTypeCommandService schoolTypeCommandService)
        {
            this.schoolTypeQueryService = schoolTypeQueryService;
            this.schoolTypeCommandService = schoolTypeCommandService;
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

        [HttpPost("GetSchoolType")]
        public async Task<IActionResult> GetSchoolType([FromBody] SchoolTypeIdDTO dTO)
        {
            try
            {
                var type = await schoolTypeQueryService.GetByIdAsync(dTO.TypeId);
                return Ok(type);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddSchoolType")]
        public async Task<IActionResult> AddSchoolType([FromBody] CreateSchoolTypeDTO dTO)
        {
            try
            {
                var type = new SchoolType
                {
                    Name = dTO.Name,
                    Description = dTO.Description
                };

                var created = schoolTypeCommandService.Create(type);

                if (!created)
                    return BadRequest("Error in creating type");

                return Ok(type);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateSchoolType")]
        public async Task<IActionResult> UpdateSchoolType([FromBody] UpdateSchoolTypeDTO dTO)
        {
            try
            {
                var type = await schoolTypeQueryService.GetByIdAsync(dTO.Id);

                if (type == null)
                    return BadRequest("There is no type to update");

                type.Name = dTO.Name;
                type.Description = dTO.Description;

                var updated = schoolTypeCommandService.Update(type);

                if (!updated)
                {
                    return BadRequest("Error in update type");
                }

                return Ok(type);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




    }
}
