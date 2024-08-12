using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.DTOs.Common;
using Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO;
using Mdaresna.Repository.IServices.UserManagement.Command;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.SchoolManagement.StudentManagement
{
    [Route("RelationType")]
    public class RelationTypeController : Controller
    {
        private readonly IRelationTypeQueryService relationTypeQueryService;
        private readonly IRelationTypeCommandService relationTypeCommandService;

        public RelationTypeController(IRelationTypeQueryService relationTypeQueryService,
                                      IRelationTypeCommandService relationTypeCommandService)
        {
            this.relationTypeQueryService = relationTypeQueryService;
            this.relationTypeCommandService = relationTypeCommandService;
        }

        [HttpGet("GetAllTypes")]
        public async Task<IActionResult> GetAllTypes()
        {
            try
            {
                var types = await relationTypeQueryService.GetAllAsync();
                return Ok(types);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetRelationType")]
        public async Task<IActionResult> GetRelationType([FromBody] RelationTypeIdDTO relationTypeId)
        {
            try
            {
                var type = await relationTypeQueryService.GetByIdAsync(relationTypeId.RelationTypeId);
                return Ok(type);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddRelation")]
        public async Task<IActionResult> CreateRelationType([FromBody] CreateRelationTypeDTO entity)
        {
            try
            {
                var type = await relationTypeQueryService.GetRelationByNameAsync(entity.Name);

                if(type != null)
                    return Ok(type);

                type = new RelationType
                {
                    Name = entity.Name
                };

                var added = relationTypeCommandService.Create(type);
                if (added)
                    return Ok(type);

                return BadRequest("Error in creating relation type");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateRelation")]
        public async Task<IActionResult> UpdateRelationType([FromBody] UpdateRelationTypeDTO entity)
        {
            try
            {
                var type = await relationTypeQueryService.GetByIdAsync(entity.Id);

                if (type == null)
                    return BadRequest("There is no type to update");

                type.Name = entity.Name;

                var updated = relationTypeCommandService.Update(type);
                if (updated)
                    return Ok(type);

                return BadRequest("Error in updateing relation type");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }







    }
}
