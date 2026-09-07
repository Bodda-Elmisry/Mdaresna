using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.Enums;
using Mdaresna.DTOs.IdentityDTO;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Middlewares;
using Mdaresna.Repository.IBServices.IdentityManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Controllers.IdentityManagement
{
    [Route("API/PhoneVerification")]
    public class PhoneVerificationController : Controller
    {
        private readonly IPhoneVerificationService verificationService;
        private readonly AppDbContext context;

        public PhoneVerificationController(
            IPhoneVerificationService verificationService,
            AppDbContext context)
        {
            this.verificationService = verificationService;
            this.context = context;
        }

        private Guid CurrentUserId
        {
            get
            {
                var claim = User.FindFirst(JwtRegisteredClaimNames.Sub) ??
                            User.FindFirst(ClaimTypes.NameIdentifier);
                return claim != null && Guid.TryParse(claim.Value, out var userId)
                    ? userId
                    : Guid.Empty;
            }
        }

        [HttpPost("ResendSms")]
        public async Task<IActionResult> ResendSms([FromBody] VerificationChallengeIdDTO dto)
        {
            if (dto.ChallengeId == Guid.Empty)
                return BadRequest("Verification challenge ID is required");

            try
            {
                return Ok(await verificationService.ResendSmsAsync(dto.ChallengeId));
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(exception.Message);
            }
        }

        [HttpPost("RequestWhatsApp")]
        public async Task<IActionResult> RequestWhatsApp(
            [FromBody] VerificationChallengeIdDTO dto)
        {
            if (dto.ChallengeId == Guid.Empty)
                return BadRequest("Verification challenge ID is required");

            try
            {
                var requestId = await verificationService.RequestWhatsAppAsync(
                    dto.ChallengeId);
                return Ok(new { RequestId = requestId });
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(exception.Message);
            }
        }

        [Authorize]
        [PermissionAuthorize("ViewWhatsAppVerificationRequests")]
        [HttpGet("GetWhatsAppRequests")]
        public async Task<IActionResult> GetWhatsAppRequests(
            WhatsAppVerificationRequestStatusEnum? status = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query =
                from request in context.WhatsAppVerificationRequests.AsNoTracking()
                join challenge in context.VerificationChallenges.AsNoTracking()
                    on request.VerificationChallengeId equals challenge.Id
                join user in context.Users.AsNoTracking()
                    on challenge.UserId equals user.Id
                where request.Deleted == false &&
                      (!status.HasValue || request.Status == status.Value)
                orderby request.RequestedAtUtc, request.Id
                select new WhatsAppVerificationRequestResultDTO
                {
                    Id = request.Id,
                    ChallengeId = challenge.Id,
                    UserId = user.Id,
                    UserName = ((user.FirstName ?? string.Empty) + " " +
                                (user.LastName ?? string.Empty)).Trim(),
                    PhoneNumber = challenge.PhoneNumber,
                    Purpose = challenge.Purpose,
                    Status = request.Status,
                    RequestedAtUtc = request.RequestedAtUtc,
                    ExpiresAtUtc = request.ExpiresAtUtc,
                    PreparedAtUtc = request.PreparedAtUtc,
                    PreparedByUserId = request.PreparedByUserId,
                    SentConfirmedAtUtc = request.SentConfirmedAtUtc,
                    SentConfirmedByUserId = request.SentConfirmedByUserId
                };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PagedResultDTO<WhatsAppVerificationRequestResultDTO>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        [Authorize]
        [PermissionAuthorize("PrepareWhatsAppVerificationMessage")]
        [HttpPost("PrepareWhatsApp/{requestId:guid}")]
        public async Task<IActionResult> PrepareWhatsApp(Guid requestId)
        {
            if (CurrentUserId == Guid.Empty)
                return Unauthorized();

            try
            {
                return Ok(await verificationService.PrepareWhatsAppAsync(
                    requestId,
                    CurrentUserId));
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(exception.Message);
            }
        }

        [Authorize]
        [PermissionAuthorize("ConfirmWhatsAppVerificationSent")]
        [HttpPost("ConfirmWhatsAppSent/{requestId:guid}")]
        public async Task<IActionResult> ConfirmWhatsAppSent(Guid requestId)
        {
            if (CurrentUserId == Guid.Empty)
                return Unauthorized();

            try
            {
                await verificationService.ConfirmWhatsAppSentAsync(
                    requestId,
                    CurrentUserId);
                return Ok("WhatsApp verification message marked as sent");
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(exception.Message);
            }
        }
    }
}
