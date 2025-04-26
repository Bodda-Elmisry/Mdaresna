using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.DTOs.CoinsManagementDTO;
using Mdaresna.DTOs.Common;
using Mdaresna.Repository.IServices.CoinsManagement.Command;
using Mdaresna.Repository.IServices.CoinsManagement.Query;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Algorithm.Distance;

namespace Mdaresna.Controllers.CoinsManagement
{
    [Route("PaymentTransaction")]
    public class PaymentTransactionController : Controller
    {
        private readonly IPaymentTransactionQueryService paymentTransactionQueryService;
        private readonly IPaymentTransactionCommandService paymentTransactionCommandService;

        public PaymentTransactionController(IPaymentTransactionQueryService paymentTransactionQueryService,
                                            IPaymentTransactionCommandService paymentTransactionCommandService)
        {
            this.paymentTransactionQueryService = paymentTransactionQueryService;
            this.paymentTransactionCommandService = paymentTransactionCommandService;
        }

        [HttpPost("GetPaymentsList")]
        public async Task<IActionResult> GetPaymentsList([FromBody] GetPaymentTransactionsDTO dto)
        {
            try
            {
                var payments = await paymentTransactionQueryService.GetPaymentTransactionsListAsync(dto.PaymentDateFrom,
                                                                                                    dto.PaymentDateTO,
                                                                                                    dto.Amount,
                                                                                                    dto.PaymentTypeId,
                                                                                                    dto.FromId,
                                                                                                    dto.FromName,
                                                                                                    dto.FromType,
                                                                                                    dto.ToId,
                                                                                                    dto.ToName,
                                                                                                    dto.ToType,
                                                                                                    dto.CoinTypeId,
                                                                                                    dto.SchoolRequestId);

                return Ok(payments);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetPayment")]
        public async Task<IActionResult> GetPayment([FromBody] PaymentTransactionIdDTO dto)
        {
            try
            {
                var payment = await paymentTransactionQueryService.GetPaymentTransactionByIdAsync(dto.PaymentTransactionId);
                return Ok(payment);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddPayment")]
        public async Task<IActionResult> AddPayment([FromBody] CreatePaymentTransactionDTO dto)
        {
            try
            {
                var payment = new PaymentTransaction
                {
                    PaymentDate = dto.PaymentDate,
                    Amount = dto.Amount,
                    PaymentTypeId = dto.PaymentTypeId,
                    FromId = $"{dto.FromType}|||{dto.FromId}|||{dto.FromName}",
                    ToId = $"{dto.ToType}|||{dto.ToId}|||{dto.ToName}",
                    CoinTypeId = dto.CoinTypeId,
                    SchoolRequestId = dto.SchoolRequestId
                };

                var created = paymentTransactionCommandService.Create(payment);

                if (!created)
                    return BadRequest("Error in adding payment");

                return Ok(await paymentTransactionQueryService.GetPaymentTransactionByIdAsync(payment.Id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdatePayment")]
        public async Task<IActionResult> UpdatePayment([FromBody] UpdatePaymentTransactionDTO dto)
        {
            try
            {
                var payment = await paymentTransactionQueryService.GetByIdAsync(dto.Id);

                if (payment != null)
                    return BadRequest("There is no payment to update");

                payment.Amount = dto.Amount;

                var updated = paymentTransactionCommandService.Update(payment);

                if (!updated)
                    return BadRequest("Error in updating payment");

                return Ok(await paymentTransactionQueryService.GetPaymentTransactionByIdAsync(payment.Id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SoftDeletePayment")]
        public async Task<IActionResult> SoftDeletePayment([FromBody] PaymentTransactionIdDTO dto)
        {
            try
            {
                var payment = await paymentTransactionQueryService.GetByIdAsync(dto.PaymentTransactionId);

                if (payment == null)
                    return BadRequest("Payment Not Found");

                payment.Deleted = true;

                var result = paymentTransactionCommandService.Update(payment);

                return result ? Ok("Payment Deleted") : BadRequest("Error in delete payment");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
