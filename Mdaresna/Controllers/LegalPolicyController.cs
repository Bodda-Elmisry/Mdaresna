using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.DTOs.LegalPolicyDTO;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Controllers
{
    [Authorize]
    [Route("API/LegalPolicy")]
    public class LegalPolicyController : Controller
    {
        private readonly AppDbContext context;

        public LegalPolicyController(AppDbContext context)
        {
            this.context = context;
        }

        private Guid CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)
                    ? userId
                    : Guid.Empty;
            }
        }

        [HttpGet("CurrentStatus")]
        public async Task<IActionResult> CurrentStatus()
        {
            if (CurrentUserId == Guid.Empty)
                return Unauthorized();

            return Ok(await BuildCurrentStatusAsync(CurrentUserId));
        }

        [HttpPost("AcceptCurrent")]
        public async Task<IActionResult> AcceptCurrent([FromBody] AcceptLegalPolicyDTO dto)
        {
            if (CurrentUserId == Guid.Empty)
                return Unauthorized();
            if (dto.PolicyVersionId == Guid.Empty)
                return BadRequest("Policy version ID is required");

            var policy = await GetCurrentPolicyQuery()
                .FirstOrDefaultAsync(item => item.Id == dto.PolicyVersionId);
            if (policy == null)
                return Conflict("The legal policy version is no longer current");

            var alreadyAccepted = await context.UserLegalPolicyAcceptances.AnyAsync(acceptance =>
                acceptance.UserId == CurrentUserId &&
                acceptance.LegalPolicyVersionId == policy.Id &&
                acceptance.RevokedAtUtc == null &&
                acceptance.Deleted == false);

            if (!alreadyAccepted)
            {
                var now = DateTime.UtcNow;
                context.UserLegalPolicyAcceptances.Add(new UserLegalPolicyAcceptance
                {
                    Id = Guid.NewGuid(),
                    UserId = CurrentUserId,
                    LegalPolicyVersionId = policy.Id,
                    AcceptedAtUtc = now,
                    AcceptedIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    AcceptedUserAgent = Request.Headers.UserAgent.ToString()[..Math.Min(
                        Request.Headers.UserAgent.ToString().Length,
                        500)],
                    CreateDate = now,
                    LastModifyDate = now
                });

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    context.ChangeTracker.Clear();
                    var acceptedByConcurrentRequest =
                        await context.UserLegalPolicyAcceptances.AnyAsync(acceptance =>
                            acceptance.UserId == CurrentUserId &&
                            acceptance.LegalPolicyVersionId == policy.Id &&
                            acceptance.RevokedAtUtc == null &&
                            acceptance.Deleted == false);

                    if (!acceptedByConcurrentRequest)
                        throw;
                }
            }

            return Ok(await BuildCurrentStatusAsync(CurrentUserId));
        }

        [HttpPost("Create")]
        [PermissionAuthorize("CreateLegalPolicy")]
        public async Task<IActionResult> Create([FromBody] CreateLegalPolicyDTO dto)
        {
            var validationError = ValidateCreateRequest(dto);
            if (validationError != null)
                return BadRequest(validationError);

            var version = dto.Version.Trim();
            var versionExists = await context.LegalPolicyVersions.AnyAsync(policy =>
                policy.Version == version && policy.Deleted == false);
            if (versionExists)
                return Conflict("A legal policy with the same version already exists");

            var now = DateTime.UtcNow;
            var policy = new LegalPolicyVersion
            {
                Id = Guid.NewGuid(),
                Version = version,
                TitleAr = dto.TitleAr.Trim(),
                TitleEn = dto.TitleEn.Trim(),
                PrivacyPolicyAr = dto.PrivacyPolicyAr.Trim(),
                PrivacyPolicyEn = dto.PrivacyPolicyEn.Trim(),
                UgcTermsAr = dto.UgcTermsAr.Trim(),
                UgcTermsEn = dto.UgcTermsEn.Trim(),
                IsActive = false,
                EffectiveDateUtc = NormalizeUtc(dto.EffectiveDateUtc ?? now),
                CreatedByUserId = CurrentUserId,
                CreateDate = now,
                LastModifyDate = now
            };

            context.LegalPolicyVersions.Add(policy);
            await context.SaveChangesAsync();
            return Ok(MapPolicy(policy));
        }

        [HttpPost("Activate/{policyId:guid}")]
        [PermissionAuthorize("ActivateLegalPolicy")]
        public async Task<IActionResult> Activate(Guid policyId)
        {
            var policy = await context.LegalPolicyVersions.FirstOrDefaultAsync(item =>
                item.Id == policyId && item.Deleted == false);
            if (policy == null)
                return NotFound("Legal policy not found");

            if (policy.EffectiveDateUtc > DateTime.UtcNow)
                return BadRequest("A policy cannot be activated before its effective date");

            if (policy.IsActive)
                return Ok(MapPolicy(policy));

            await using var transaction = await context.Database.BeginTransactionAsync();
            var activePolicies = await context.LegalPolicyVersions
                .Where(item => item.IsActive && item.Deleted == false)
                .ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var activePolicy in activePolicies)
            {
                activePolicy.IsActive = false;
                activePolicy.LastModifyDate = now;
            }

            await context.SaveChangesAsync();

            policy.IsActive = true;
            policy.LastModifyDate = now;
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(MapPolicy(policy));
        }

        [HttpDelete("Delete/{policyId:guid}")]
        [PermissionAuthorize("DeleteLegalPolicy")]
        public async Task<IActionResult> Delete(Guid policyId)
        {
            var policy = await context.LegalPolicyVersions.FirstOrDefaultAsync(item =>
                item.Id == policyId && item.Deleted == false);
            if (policy == null)
                return NotFound("Legal policy not found");

            if (policy.IsActive)
                return Conflict("Activate another policy before deleting the active policy");

            var hasAcceptanceHistory = await context.UserLegalPolicyAcceptances.AnyAsync(acceptance =>
                acceptance.LegalPolicyVersionId == policy.Id);
            if (hasAcceptanceHistory)
                return Conflict("A policy with acceptance history cannot be deleted");

            policy.Deleted = true;
            policy.LastModifyDate = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return Ok("Legal policy deleted");
        }

        [HttpGet("GetPolicies")]
        [PermissionAuthorize("ViewLegalPolicies")]
        public async Task<IActionResult> GetPolicies(int pageNumber = 1, int pageSize = 20)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);
            var query = context.LegalPolicyVersions
                .AsNoTracking()
                .Where(policy => policy.Deleted == false)
                .OrderByDescending(policy => policy.CreateDate)
                .ThenByDescending(policy => policy.Id);

            var totalCount = await query.CountAsync();
            var policies = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(policy => new LegalPolicyVersionResultDTO
                {
                    Id = policy.Id,
                    Version = policy.Version,
                    TitleAr = policy.TitleAr,
                    TitleEn = policy.TitleEn,
                    PrivacyPolicyAr = policy.PrivacyPolicyAr,
                    PrivacyPolicyEn = policy.PrivacyPolicyEn,
                    UgcTermsAr = policy.UgcTermsAr,
                    UgcTermsEn = policy.UgcTermsEn,
                    IsActive = policy.IsActive,
                    EffectiveDateUtc = policy.EffectiveDateUtc,
                    CreateDate = policy.CreateDate,
                    CreatedByUserId = policy.CreatedByUserId,
                    ActiveAcceptancesCount = context.UserLegalPolicyAcceptances.Count(acceptance =>
                        acceptance.LegalPolicyVersionId == policy.Id &&
                        acceptance.RevokedAtUtc == null &&
                        acceptance.Deleted == false)
                })
                .ToListAsync();

            return Ok(new PagedResultDTO<LegalPolicyVersionResultDTO>
            {
                Items = policies,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        [HttpPost("RevokeAcceptance")]
        [PermissionAuthorize("RevokeLegalPolicyAcceptance")]
        public async Task<IActionResult> RevokeAcceptance(
            [FromBody] RevokeLegalPolicyAcceptanceDTO dto)
        {
            if (dto.UserId == Guid.Empty)
                return BadRequest("User ID is required");

            var policyId = dto.PolicyVersionId;
            if (!policyId.HasValue)
            {
                policyId = await GetCurrentPolicyQuery()
                    .Select(policy => (Guid?)policy.Id)
                    .FirstOrDefaultAsync();
            }

            if (!policyId.HasValue)
                return BadRequest("There is no active legal policy");

            var acceptance = await context.UserLegalPolicyAcceptances
                .Where(item => item.UserId == dto.UserId &&
                               item.LegalPolicyVersionId == policyId.Value &&
                               item.RevokedAtUtc == null &&
                               item.Deleted == false)
                .OrderByDescending(item => item.AcceptedAtUtc)
                .FirstOrDefaultAsync();

            if (acceptance == null)
                return NotFound("An active acceptance was not found");

            acceptance.RevokedAtUtc = DateTime.UtcNow;
            acceptance.RevokedByUserId = CurrentUserId;
            acceptance.RevocationReason = string.IsNullOrWhiteSpace(dto.Reason)
                ? null
                : dto.Reason.Trim()[..Math.Min(dto.Reason.Trim().Length, 500)];
            acceptance.LastModifyDate = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return Ok("Legal policy acceptance revoked");
        }

        [HttpGet("GetAcceptances")]
        [PermissionAuthorize("ViewLegalPolicyAcceptances")]
        public async Task<IActionResult> GetAcceptances(
            Guid? policyVersionId = null,
            Guid? userId = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var query =
                from acceptance in context.UserLegalPolicyAcceptances.AsNoTracking()
                join policy in context.LegalPolicyVersions.AsNoTracking()
                    on acceptance.LegalPolicyVersionId equals policy.Id
                join user in context.Users.AsNoTracking()
                    on acceptance.UserId equals user.Id
                where acceptance.Deleted == false
                      && (!policyVersionId.HasValue || policy.Id == policyVersionId.Value)
                      && (!userId.HasValue || user.Id == userId.Value)
                orderby acceptance.AcceptedAtUtc descending, acceptance.Id descending
                select new LegalPolicyAcceptanceResultDTO
                {
                    Id = acceptance.Id,
                    UserId = user.Id,
                    UserName = ((user.FirstName ?? string.Empty) + " " +
                                (user.LastName ?? string.Empty)).Trim(),
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    PolicyVersionId = policy.Id,
                    PolicyVersion = policy.Version,
                    AcceptedAtUtc = acceptance.AcceptedAtUtc,
                    AcceptedIpAddress = acceptance.AcceptedIpAddress,
                    RevokedAtUtc = acceptance.RevokedAtUtc,
                    RevokedByUserId = acceptance.RevokedByUserId,
                    RevocationReason = acceptance.RevocationReason
                };

            var totalCount = await query.CountAsync();
            var acceptances = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PagedResultDTO<LegalPolicyAcceptanceResultDTO>
            {
                Items = acceptances,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        private IQueryable<LegalPolicyVersion> GetCurrentPolicyQuery()
        {
            var now = DateTime.UtcNow;
            return context.LegalPolicyVersions
                .Where(policy => policy.IsActive &&
                                 policy.Deleted == false &&
                                 policy.EffectiveDateUtc <= now)
                .OrderByDescending(policy => policy.CreateDate);
        }

        private async Task<LegalPolicyAcceptanceStatusDTO> BuildCurrentStatusAsync(Guid userId)
        {
            var policy = await GetCurrentPolicyQuery().AsNoTracking().FirstOrDefaultAsync();
            if (policy == null)
                return new LegalPolicyAcceptanceStatusDTO();

            var acceptance = await context.UserLegalPolicyAcceptances
                .AsNoTracking()
                .Where(item => item.UserId == userId &&
                               item.LegalPolicyVersionId == policy.Id &&
                               item.RevokedAtUtc == null &&
                               item.Deleted == false)
                .OrderByDescending(item => item.AcceptedAtUtc)
                .FirstOrDefaultAsync();

            return new LegalPolicyAcceptanceStatusDTO
            {
                HasActivePolicy = true,
                Accepted = acceptance != null,
                AcceptedAtUtc = acceptance?.AcceptedAtUtc,
                Policy = MapPolicy(policy)
            };
        }

        private static LegalPolicyVersionResultDTO MapPolicy(LegalPolicyVersion policy)
        {
            return new LegalPolicyVersionResultDTO
            {
                Id = policy.Id,
                Version = policy.Version,
                TitleAr = policy.TitleAr,
                TitleEn = policy.TitleEn,
                PrivacyPolicyAr = policy.PrivacyPolicyAr,
                PrivacyPolicyEn = policy.PrivacyPolicyEn,
                UgcTermsAr = policy.UgcTermsAr,
                UgcTermsEn = policy.UgcTermsEn,
                IsActive = policy.IsActive,
                EffectiveDateUtc = policy.EffectiveDateUtc,
                CreateDate = policy.CreateDate,
                CreatedByUserId = policy.CreatedByUserId
            };
        }

        private static string? ValidateCreateRequest(CreateLegalPolicyDTO dto)
        {
            if (dto == null)
                return "Policy data is required";
            if (string.IsNullOrWhiteSpace(dto.Version) || dto.Version.Trim().Length > 50)
                return "A policy version of at most 50 characters is required";
            if (string.IsNullOrWhiteSpace(dto.TitleAr) || dto.TitleAr.Trim().Length > 300 ||
                string.IsNullOrWhiteSpace(dto.TitleEn) || dto.TitleEn.Trim().Length > 300)
                return "Arabic and English policy titles are required";
            if (string.IsNullOrWhiteSpace(dto.PrivacyPolicyAr) &&
                string.IsNullOrWhiteSpace(dto.PrivacyPolicyEn))
                return "Privacy policy content is required";
            if (string.IsNullOrWhiteSpace(dto.UgcTermsAr) &&
                string.IsNullOrWhiteSpace(dto.UgcTermsEn))
                return "Terms of use content is required";
            return null;
        }

        private static DateTime NormalizeUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            return (Math.Max(1, pageNumber), Math.Clamp(pageSize, 1, 100));
        }
    }
}
