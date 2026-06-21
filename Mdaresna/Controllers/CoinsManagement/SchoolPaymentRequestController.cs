using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.DTOs.CoinsManagementDTO;
using Mdaresna.DTOs.Common;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IServices.CoinsManagement.Command;
using Mdaresna.Repository.IServices.CoinsManagement.Query;
using Mdaresna.Repository.IServices.IdentityManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using Mdaresna.Repository.IServices.UserManagement.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Controllers.CoinsManagement
{
    [Route("SchoolPaymentRequest")]
    [Authorize]
    public class SchoolPaymentRequestController : Controller
    {
        private readonly ISchoolPaymentRequestCommandService schoolPaymentRequestCommandService;
        private readonly ISchoolPaymentRequestQueryService schoolPaymentRequestQueryService;
        private readonly IPaymentTransactionCommandService paymentTransactionCommandService;
        private readonly ISchoolQueryService schoolQueryService;
        private readonly ISchoolCommandService schoolCommandService;
        private readonly ICoinTypeQueryService coinTypeQueryService;
        private readonly IUserPermissionQueryService userPermissionQueryService;
        private readonly INotificationFactory notificationFactory;
        private readonly IUserDeviceQueryService userDeviceQueryService;
        private readonly ISchoolAccessValidator schoolAccessValidator;

        public SchoolPaymentRequestController(ISchoolPaymentRequestCommandService schoolPaymentRequestCommandService,
                                              ISchoolPaymentRequestQueryService schoolPaymentRequestQueryService,
                                              IPaymentTransactionCommandService paymentTransactionCommandService,
                                              ISchoolQueryService schoolQueryService,
                                              ISchoolCommandService schoolCommandService,
                                              ICoinTypeQueryService coinTypeQueryService,
                                              IUserPermissionQueryService userPermissionQueryService,
                                              INotificationFactory notificationFactory,
                                              IUserDeviceQueryService userDeviceQueryService,
                                              ISchoolAccessValidator schoolAccessValidator)
        {
            this.schoolPaymentRequestCommandService = schoolPaymentRequestCommandService;
            this.schoolPaymentRequestQueryService = schoolPaymentRequestQueryService;
            this.paymentTransactionCommandService = paymentTransactionCommandService;
            this.schoolQueryService = schoolQueryService;
            this.schoolCommandService = schoolCommandService;
            this.coinTypeQueryService = coinTypeQueryService;
            this.userPermissionQueryService = userPermissionQueryService;
            this.notificationFactory = notificationFactory;
            this.userDeviceQueryService = userDeviceQueryService;
            this.schoolAccessValidator = schoolAccessValidator;
        }

        private Guid CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub) 
                                   ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;
            }
        }

        private bool IsApplicationManager
        {
            get
            {
                var userTypeClaim = User.FindFirst("user_type");
                return userTypeClaim != null && userTypeClaim.Value == ((int)UserTypeEnum.ApplicationManager).ToString();
            }
        }

        [HttpPost("GetRequests")]
        public async Task<IActionResult> GetRequests([FromBody] GetSchoolPaymentRequestsDTO dTO)
        {
            try
            {
                if (!IsApplicationManager && (!dTO.SchoolId.HasValue || !await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, dTO.SchoolId.Value)))
                {
                    return Forbid();
                }

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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetRequest")]
        public async Task<IActionResult> GetRequest([FromBody] PaymentRequestIdDTO dTO)
        {
            try
            {
                var requestEntity = await schoolPaymentRequestQueryService.GetByIdAsync(dTO.RequestId);
                if (requestEntity == null)
                {
                    return BadRequest("Request not found");
                }

                if (!IsApplicationManager && !await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, requestEntity.SchoolId))
                {
                    return Forbid();
                }

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

            try
            {
                if (!IsApplicationManager && !await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, dTO.SchoolId))
                {
                    return Forbid();
                }

                var request = new SchoolPaymentRequest
                {
                    RequestDate = dTO.RequestDate,
                    TransfareCode = dTO.TransfareCode,
                    TransfareDate = dTO.TransfareDate,
                    TransfareAmount = dTO.TransfareAmount,
                    PaymentTypeId = dTO.PaymentTypeId,
                    SchoolId = dTO.SchoolId,
                    Approvied = null,
                    ApproviedById = null
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

                if (!IsApplicationManager && !await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, request.SchoolId))
                {
                    return Forbid();
                }

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
                if (!IsApplicationManager)
                {
                    return Forbid();
                }

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

                //var paymentCoinsCount = payment.Amount / coinType.Value;

                school.AvailableCoins += Convert.ToInt32(payment.Amount);

                var schoolUpdated = schoolCommandService.Update(school);

                request.Approvied = true;
                request.ApproviedById = CurrentUserId;

                var approved = schoolPaymentRequestCommandService.Update(request);

                if (!approved)
                    return BadRequest("Error in approving request");


                var usersIds = await userPermissionQueryService.GetPermissionUsersIds(Guid.Parse("9DD22E15-9701-492B-AE20-985B8927F3BF"), school.Id);
                var userIdsList = usersIds.ToList();
                userIdsList.Add(school.SchoolAdminId);
                var notificationProvider = notificationFactory.GetProvider(NotificationProvidersEnum.Mobile);
                var devices = await userDeviceQueryService.GetUsersDevicesAsync(userIdsList);
                if (devices.Count() > 0)
                {
                    var message = $"Units changed in school '{school.Name}'|{school.AvailableCoins}|{school.Id}";
                    var tokens = devices.Select(d => d.FcmToken).ToList();
                    await notificationProvider.SendToMultiUsersAsync(tokens, "Units Changed", message);
                }
                return Ok("Request approved");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("RejectRequest")]
        public async Task<IActionResult> RejectRequest([FromBody] RejectPaymentRequestDTO dTO)
        {
            try
            {
                if (!IsApplicationManager)
                {
                    return Forbid();
                }

                var request = await schoolPaymentRequestQueryService.GetByIdAsync(dTO.RequestId);

                if (request == null)
                    return BadRequest("There is no request to reject");

                request.Approvied = false;
                request.ApproviedById = CurrentUserId;

                var updated = schoolPaymentRequestCommandService.Update(request);

                if (!updated)
                    return BadRequest("Error in rejection");

                return Ok("Request rejected");
            }
            catch (Exception ex)
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

                if (!IsApplicationManager && !await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, request.SchoolId))
                {
                    return Forbid();
                }

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

        [HttpPost("SoftDeleteRequest")]
        public async Task<IActionResult> SoftDeleteRequest([FromBody] PaymentRequestIdDTO dTO)
        {
            try
            {
                var request = await schoolPaymentRequestQueryService.GetByIdAsync(dTO.RequestId);

                if (request == null)
                    return BadRequest("There is no request to delete");

                if (!IsApplicationManager && !await schoolAccessValidator.CanAccessSchoolAsync(CurrentUserId, request.SchoolId))
                {
                    return Forbid();
                }

                request.Deleted = true;

                var deleted = schoolPaymentRequestCommandService.Update(request);

                return deleted ? Ok("Request Deleted") : BadRequest("Error in delete request");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
