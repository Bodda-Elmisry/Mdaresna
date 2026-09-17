using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Application.Abstractions.Messaging;
using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Api.Controllers.Internal;

[ApiController]
[AllowAnonymous]
[PlatformInternalService]
[Route("api/platform/v1/internal/school-user-identities")]
public sealed class SchoolUserIdentitiesController(
    IdentityDbContext identityDb,
    ISchoolRegistrationRepository schools,
    ISharedIdentityAccountContactReader accountContacts,
    IPlatformSmsSender smsSender,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost("resolve")]
    public async Task<IActionResult> Resolve([FromBody] ResolveSchoolUserIdentityRequest request,
        CancellationToken cancellationToken)
    {
        if (!Authorized()) return Unauthorized();
        if (!TryNormalizePhone(request.Phone, out var phone) || request.PlatformSchoolId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Trim().Length > 200)
            return BadRequest();
        var school = await schools.FindByIdAsync(SchoolId.From(request.PlatformSchoolId), cancellationToken);
        if (school is null || school.Status != SchoolLifecycleStatus.Active) return NotFound();

        var identifier = await identityDb.LoginIdentifiers.Include(x => x.Account)
            .SingleOrDefaultAsync(x => x.Type == LoginIdentifierType.Phone && x.SchoolId == null &&
                x.NormalizedValue == phone, cancellationToken);
        Account account;
        var now = DateTimeOffset.UtcNow;
        if (identifier is null)
        {
            account = new Account
            {
                Id = Guid.NewGuid(), DisplayName = request.DisplayName.Trim(), Status = AccountStatus.PendingVerification,
                CreatedAtUtc = now, UpdatedAtUtc = now
            };
            identifier = new LoginIdentifier
            {
                Id = Guid.NewGuid(), AccountId = account.Id, Account = account, Type = LoginIdentifierType.Phone,
                NormalizedValue = phone, DisplayValue = phone, SchoolId = null, IsPrimary = true,
                IsVerified = false, CreatedAtUtc = now
            };
            account.LoginIdentifiers.Add(identifier);
            identityDb.Accounts.Add(account);
            identityDb.SecurityEvents.Add(new IdentitySecurityEvent
            {
                Id = Guid.NewGuid(), AccountId = account.Id, EventType = "school.user.identity.provisioned",
                Succeeded = true, OccurredAtUtc = now
            });
            try { await identityDb.SaveChangesAsync(cancellationToken); }
            catch (DbUpdateException) { return Conflict(); }
        }
        else
        {
            if (!identifier.IsPrimary) return Conflict(ApiResponse<object?>.Failure(409,
                "school_user.primary_phone_required", "Use the account's primary phone.",
                correlationId: HttpContext.TraceIdentifier));
            account = identifier.Account;
            if (account.Status != AccountStatus.Active && account.Status != AccountStatus.PendingVerification)
                return Conflict(ApiResponse<object?>.Failure(409, "school_user.account_unavailable",
                    "The central account is unavailable.", correlationId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<SchoolUserIdentityResponse>.Success(new(account.Id, account.DisplayName ?? request.DisplayName.Trim(),
            identifier.DisplayValue, identifier.IsVerified), correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("send-invitation")]
    public async Task<IActionResult> SendInvitation([FromBody] SendSchoolUserInvitationRequest request,
        CancellationToken cancellationToken)
    {
        if (!Authorized()) return Unauthorized();
        if (request.PlatformSchoolId == Guid.Empty || request.PlatformAccountId == Guid.Empty) return BadRequest();
        var school = await schools.FindByIdAsync(SchoolId.From(request.PlatformSchoolId), cancellationToken);
        if (school is null || school.Status != SchoolLifecycleStatus.Active) return NotFound();
        var phone = await accountContacts.GetPrimaryPhoneAsync(IdentityAccountId.From(request.PlatformAccountId), cancellationToken);
        if (phone is null) return NotFound();
        await smsSender.SendAsync(phone,
            $"تمت إضافتك إلى {school.DisplayName}. اسم الدخول: {request.FullLogin}. افتح تطبيق مدارسنا واختر تفعيل الحساب لطلب رمز التفعيل.",
            "school-user-invitation", request.PlatformSchoolId, cancellationToken);
        return NoContent();
    }

    [HttpPost("primary-contact")]
    public async Task<IActionResult> GetPrimaryContact([FromBody] GetSchoolUserPrimaryContactRequest request,
        CancellationToken cancellationToken)
    {
        if (!Authorized()) return Unauthorized();
        if (request.PlatformSchoolId == Guid.Empty || request.PlatformAccountId == Guid.Empty) return BadRequest();
        var school = await schools.FindByIdAsync(SchoolId.From(request.PlatformSchoolId), cancellationToken);
        if (school is null || school.Status != SchoolLifecycleStatus.Active) return NotFound();

        var phone = await identityDb.LoginIdentifiers.AsNoTracking()
            .Where(x => x.AccountId == request.PlatformAccountId && x.SchoolId == null &&
                x.Type == LoginIdentifierType.Phone)
            .OrderByDescending(x => x.IsPrimary).ThenBy(x => x.CreatedAtUtc).ThenBy(x => x.Id)
            .Select(x => new SchoolUserPrimaryContactResponse(x.Id, x.DisplayValue, x.IsVerified))
            .FirstOrDefaultAsync(cancellationToken);
        return phone is null
            ? NotFound(ApiResponse<object?>.Failure(404, "school_user.primary_phone_not_found",
                "The account primary phone was not found.", correlationId: HttpContext.TraceIdentifier))
            : Ok(ApiResponse<SchoolUserPrimaryContactResponse>.Success(phone,
                correlationId: HttpContext.TraceIdentifier));
    }

    private bool Authorized()
    {
        var configured = configuration["InternalServices:ApiKey"];
        var supplied = Request.Headers["X-Mdaresna-Internal-Key"].ToString();
        if (string.IsNullOrWhiteSpace(configured)) return false;
        var left = Encoding.UTF8.GetBytes(configured); var right = Encoding.UTF8.GetBytes(supplied);
        return left.Length == right.Length && CryptographicOperations.FixedTimeEquals(left, right);
    }

    private static bool TryNormalizePhone(string? value, out string phone)
    {
        phone = value?.Trim() ?? string.Empty;
        return phone.Length is >= 8 and <= 16 && phone.All(char.IsAsciiDigit);
    }
}

public sealed record ResolveSchoolUserIdentityRequest(Guid PlatformSchoolId,
    [Required] string Phone, [Required, MaxLength(200)] string DisplayName);
public sealed record SchoolUserIdentityResponse(Guid AccountId, string DisplayName,
    string PrimaryPhone, bool PhoneVerified);
public sealed record SendSchoolUserInvitationRequest(Guid PlatformSchoolId, Guid PlatformAccountId,
    [Required, MaxLength(140)] string FullLogin);
public sealed record GetSchoolUserPrimaryContactRequest(Guid PlatformSchoolId, Guid PlatformAccountId);
public sealed record SchoolUserPrimaryContactResponse(Guid Id, string PrimaryPhone, bool PhoneVerified);
