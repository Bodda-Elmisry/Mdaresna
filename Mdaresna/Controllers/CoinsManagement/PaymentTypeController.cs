using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.DTOs.CoinsManagementDTO;
using Mdaresna.DTOs.Common;
using Mdaresna.Repository.IServices.CoinsManagement.Command;
using Mdaresna.Repository.IServices.CoinsManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.CoinsManagement
{
    [Route("PaymentType")]
    public class PaymentTypeController : Controller
    {
        private readonly IPaymentTypeCommandService paymentTypeCommandService;
        private readonly IPaymentTypeQueryService paymentTypeQueryService;

        public PaymentTypeController(IPaymentTypeCommandService paymentTypeCommandService,
                                     IPaymentTypeQueryService paymentTypeQueryService)
        {
            this.paymentTypeCommandService = paymentTypeCommandService;
            this.paymentTypeQueryService = paymentTypeQueryService;
        }

        [HttpPost("GetTypes")]
        public async Task<IActionResult> GetTypes([FromBody] GetPaymentTypesDTO dTO)
        {
            try
            {
                var types = await paymentTypeQueryService.GetPaumentTypesAsync(dTO.Name, dTO.Notes, dTO.IsActive);
                return Ok(types);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetType")]
        public async Task<IActionResult> GetType([FromBody] PaymentTypeIdDTO dTO)
        {
            try
            {
                var type = await paymentTypeQueryService.GetByIdAsync(dTO.PaymentTypeId);
                return Ok(type);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreateType")]
        public async Task<IActionResult> CreateType([FromBody] CreatePaymentTypeDTO dTO)
        {
            try
            {
                var type = await paymentTypeQueryService.GetPaumentTypeByNameAsync(dTO.Name);

                if (type != null && type.IsActive == false)
                {
                    type.IsActive = true;

                    var updated = paymentTypeCommandService.Update(type);
                    if (!updated)
                        return BadRequest("Error in activating payment type");

                    return Ok(type);
                }
                else if (type != null && type.IsActive == true)
                    return BadRequest("This payment type already exist");

                var nType = new PaymentType
                {
                    Name = dTO.Name,
                    Note = dTO.Notes,
                    IsActive = dTO.IsActive,
                };

                var added = paymentTypeCommandService.Create(nType);

                if (!added)
                    return BadRequest("Error in adding payment type");

                return Ok(nType);
                    


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateType")]
        public async Task<IActionResult> UpdateType([FromBody] UpdatePaymentTypeDTO dTO)
        {
            try
            {
                var type = await paymentTypeQueryService.GetByIdAsync(dTO.Id);

                if (type == null)
                    return BadRequest("There id no type to update");

                type.Name = dTO.Name;
                type.IsActive = dTO.IsActive;
                type.Note = dTO.Notes;

                var updated = paymentTypeCommandService.Update(type);

                if (!updated)
                    return BadRequest("Error in updating type");

                return Ok(type);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ChangeTypeActivation")]
        public async Task<IActionResult> ChangeTypeActivation([FromBody] ChangeTypeActivationDTO dTO)
        {
            try
            {
                var type = await paymentTypeQueryService.GetByIdAsync(dTO.PaymentTypeId);

                if (type == null)
                    return BadRequest("There id no type to change activation");

                type.IsActive = dTO.IsActive;

                var updated = paymentTypeCommandService.Update(type);

                if (!updated)
                    return BadRequest("Error in changing activation to type");

                return Ok("Activation changed");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeleteType")]
        public async Task<IActionResult> SoftDeleteType([FromBody] PaymentTypeIdDTO dTO)
        {
            try
            {
                var type = await paymentTypeQueryService.GetByIdAsync(dTO.PaymentTypeId);

                if (type == null)
                    return BadRequest("There id no type to delete");

                type.Deleted = true;

                var updated = paymentTypeCommandService.Update(type);

                return updated ? Ok("Type Deleted") : BadRequest("Error in delete type");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }






    }
}
