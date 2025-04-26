using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.DTOs.CoinsManagementDTO;
using Mdaresna.DTOs.Common;
using Mdaresna.Repository.IServices.CoinsManagement.Command;
using Mdaresna.Repository.IServices.CoinsManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.CoinsManagement
{
    [Route("CoinType")]
    public class CoinTypeController : Controller
    {
        private readonly ICoinTypeCommandService coinTypeCommandService;
        private readonly ICoinTypeQueryService coinTypeQueryService;

        public CoinTypeController(ICoinTypeCommandService coinTypeCommandService,
                                  ICoinTypeQueryService coinTypeQueryService)
        {
            this.coinTypeCommandService = coinTypeCommandService;
            this.coinTypeQueryService = coinTypeQueryService;
        }

        [HttpPost("GetCoinTypesList")]
        public async Task<IActionResult> GetCoinTypesList([FromBody] GetCoinTypesDTO dTO)
        {
            try
            {
                var types = await coinTypeQueryService.GetCoinTypesListAsync(dTO.Name,
                                                                             dTO.Value,
                                                                             dTO.Notes);

                return Ok(types);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetCoinType")]
        public async Task<IActionResult> GetCoinType([FromBody] CoinTypeIdDTO dTO)
        {
            try
            {
                var type = await coinTypeQueryService.GetByIdAsync(dTO.CoinTypeId);

                return Ok(type);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddCoinType")]
        public async Task<IActionResult> AddCoinType([FromBody] CreateCoinTypeDTO dTO)
        {
            try
            {
                var type = new CoinType
                {
                    Name = dTO.Name,
                    Value = dTO.Value,
                    Note = dTO.Notes
                };

                var added = coinTypeCommandService.Create(type);

                if (!added)
                    return BadRequest("Error in creating coin type");

                return Ok(type);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateCoinType")]
        public async Task<IActionResult> UpdateCoinType([FromBody] UpdateCoinTypeDTO dTO)
        {
            try
            {
                var type = await coinTypeQueryService.GetByIdAsync(dTO.Id);

                if (type == null)
                    return BadRequest("There is no coin type to update");

                type.Name = dTO.Name;
                type.Value = dTO.Value;
                type.Note = dTO.Notes;

                var updated = coinTypeCommandService.Update(type);

                if (!updated)
                    return BadRequest("Error in updating coin type");

                return Ok(type);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteCoinType")]
        public async Task<IActionResult> SoftDeleteCoinType([FromBody] CoinTypeIdDTO dTO)
        {
            try
            {
                var type = await coinTypeQueryService.GetByIdAsync(dTO.CoinTypeId);

                if (type == null)
                    return BadRequest("There is no coin type to delete");

                type.Deleted = true;

                var deleted = coinTypeCommandService.Update(type);

                return deleted ? Ok("Coin Type Deleted") : BadRequest("Error in delete coin type");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }





    }
}
