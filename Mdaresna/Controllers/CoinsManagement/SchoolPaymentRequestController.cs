using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.DTOs.CoinsManagementDTO;
using Mdaresna.DTOs.Common;
using Mdaresna.Repository.IServices.CoinsManagement.Command;
using Mdaresna.Repository.IServices.CoinsManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.CoinsManagement
{
    [Route("SchoolPaymentRequest")]
    public class SchoolPaymentRequestController : Controller
    {
        private readonly ISchoolPaymentRequestCommandService schoolPaymentRequestCommandService;
        private readonly ISchoolPaymentRequestQueryService schoolPaymentRequestQueryService;
        private readonly IPaymentTransactionCommandService paymentTransactionCommandService;
        private readonly ISchoolQueryService schoolQueryService;
        private readonly ISchoolCommandService schoolCommandService;
        private readonly ICoinTypeQueryService coinTypeQueryService;

        public SchoolPaymentRequestController(ISchoolPaymentRequestCommandService schoolPaymentRequestCommandService,
                                              ISchoolPaymentRequestQueryService schoolPaymentRequestQueryService,
                                              IPaymentTransactionCommandService paymentTransactionCommandService,
                                              ISchoolQueryService schoolQueryService,
                                              ISchoolCommandService schoolCommandService,
                                              ICoinTypeQueryService coinTypeQueryService)
        {
            this.schoolPaymentRequestCommandService = schoolPaymentRequestCommandService;
            this.schoolPaymentRequestQueryService = schoolPaymentRequestQueryService;
            this.paymentTransactionCommandService = paymentTransactionCommandService;
            this.schoolQueryService = schoolQueryService;
            this.schoolCommandService = schoolCommandService;
            this.coinTypeQueryService = coinTypeQueryService;
        }

        [HttpPost("GetRequests")]
        public async Task<IActionResult> GetRequests([FromBody] GetSchoolPaymentRequestsDTO dTO)
        {
            try
            {
                var requests = await schoolPaymentRequestQueryService.GetSchoolPaymentRequestsListAsync(dTO.RequestDateFrom,
                                                                                                        dTO.RequestDateTo,
                                                                                                        dTO.TransfareCode,
                                                                                                        dTO.TransfareDateFrom,
                                                                                                        dTO.TransfareDateTo,
                                                                                                        dTO.TransfareAmount,
                                                                                                        dTO.PaymentTypeId,
                                                                                                        dTO.SchoolId,
                                                                                                        dTO.Approvied,
                                                                                                        dTO.ApproviedById);

                return Ok(requests);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetRequest")]
        public async Task<IActionResult> GetRequest([FromBody] PaymentRequestIdDTO dTO)
        {
            try
            {
                var request = await schoolPaymentRequestQueryService.GetSchoolPaymentRequestViewAsync(dTO.RequestId);

                return Ok(request);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }

        [HttpPost("AddRequest")]
        public async Task<IActionResult> AddRequest([FromBody] CreateSchoolPaymentRequestDTO dTO)
        {
            return BadRequest("Error while adding request");

            try
            {
                var request = new SchoolPaymentRequest
                {
                    RequestDate = dTO.RequestDate,
                    TransfareCode = dTO.TransfareCode,
                    TransfareDate = dTO.RequestDate,
                    TransfareAmount = dTO.TransfareAmount,
                    PaymentTypeId = dTO.PaymentTypeId,
                    SchoolId = dTO.SchoolId,
                    Approvied = dTO.Approvied,
                    ApproviedById = dTO.ApproviedById
                };

                var added = schoolPaymentRequestCommandService.Create(request);

                if (!added)
                    return BadRequest("Error while adding request");

                return Ok(await schoolPaymentRequestQueryService.GetSchoolPaymentRequestViewAsync(request.Id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }

        [HttpPost("UpdateRequest")]
        public async Task<IActionResult> UpdateRequest([FromBody] UpdateSchoolPaymentRequestDTO dTO)
        {
            try
            {
                var request = await schoolPaymentRequestQueryService.GetByIdAsync(dTO.Id);

                if (request == null)
                    return BadRequest("There is no request to update");

                if (request.Approvied != null)
                    return BadRequest("Can't update request because it's already reviewd");

                request.TransfareCode = dTO.TransfareCode;
                request.TransfareDate = dTO.TransfareDate;
                request.TransfareAmount = dTO.TransfareAmount;
                request.PaymentTypeId = dTO.PaymentTypeId;

                var updated = schoolPaymentRequestCommandService.Update(request);

                if (!updated)
                    return BadRequest("Error in updating request");

                return Ok(await schoolPaymentRequestQueryService.GetSchoolPaymentRequestViewAsync(dTO.Id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }

        [HttpPost("ApproveRequest")]
        public async Task<IActionResult> ApprovePaymentRequest([FromBody] ApprovePaymentRequestDTO dTO)
        {
            try
            {
                var request = await schoolPaymentRequestQueryService.GetByIdAsync(dTO.RequestId);

                if (request == null)
                    return BadRequest("There is no request to approve");

                var school = await schoolQueryService.GetByIdAsync(request.SchoolId);

                if (school == null || (school != null && school.Active == false))
                    return BadRequest("Can't fiend the request school, It's code be not active");

                var payment = new PaymentTransaction
                {
                    PaymentDate = dTO.PaymentDate,
                    Amount = dTO.PaymentAmount,
                    PaymentTypeId = request.PaymentTypeId,
                    FromId = $"School|||{school.Id}|||{school.Name}",
                    ToId = $"Application|||{dTO.ToId}|||{dTO.ToName}",
                    CoinTypeId = school.CoinTypeId.Value,
                    SchoolRequestId = request.Id
                };

                var paymentAdded = paymentTransactionCommandService.Create(payment);

                if (!paymentAdded)
                    return BadRequest("Error in adding payment");

                var coinType = await coinTypeQueryService.GetByIdAsync(payment.CoinTypeId);

                var paymentCoinsCount = payment.Amount / coinType.Value;

                school.AvailableCoins += Convert.ToInt32(paymentCoinsCount);

                var schoolUpdated = schoolCommandService.Update(school);

                request.Approvied = true;
                request.ApproviedById = Guid.Parse(dTO.ToId);

                var approved = schoolPaymentRequestCommandService.Update(request);

                if (!approved)
                    return BadRequest("Error in approving request");

                return Ok("Request approved");

            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("DeleteRequest")]
        public async Task<IActionResult> DeleteRequest([FromBody] PaymentRequestIdDTO dTO)
        {
            try
            {
                var request = await schoolPaymentRequestQueryService.GetByIdAsync(dTO.RequestId);

                if (request == null)
                    return BadRequest("There is no request to delete");

                if (request.Approvied != null)
                    return BadRequest("Can't delete request because it's already reviewd");

                var deleted = await schoolPaymentRequestCommandService.DeleteAsync(request);

                if (!deleted)
                    return BadRequest("Error while deleting request");

                return Ok("Request deleted");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }



    }
}
