using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
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
[Route("api/platform/v1/internal/school-owner-activation-codes")]
public sealed class SchoolOwnerActivationCodesController(
    ISharedIdentityAccountContactReader accountContacts,
    ISchoolRegistrationRepository schools,
    IPlatformSmsSender smsSender,
    IdentityDbContext identityDb,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Send(
        [FromBody] SendSchoolOwnerActivationCodeRequest request,
        CancellationToken cancellationToken)
    {
        var configuredKey = configuration["InternalServices:ApiKey"];
        var suppliedKey = Request.Headers["X-Mdaresna-Internal-Key"].ToString();
        if (string.IsNullOrWhiteSpace(configuredKey) || !FixedEquals(configuredKey, suppliedKey))
            return Unauthorized();
        if (request.PlatformAccountId == Guid.Empty || request.PlatformSchoolId == Guid.Empty)
            return BadRequest();
        var school = await schools.FindByIdAsync(SchoolId.From(request.PlatformSchoolId), cancellationToken);
        if (school is null || school.Status != SchoolLifecycleStatus.Active) return NotFound();
        var phone = await accountContacts.GetPrimaryPhoneAsync(
            IdentityAccountId.From(request.PlatformAccountId), cancellationToken);
        if (phone is null) return NotFound();
        await smsSender.SendAsync(phone,
            $"رمز تفعيل حسابك على مدارسنا: {request.Code}. اسم الدخول: {request.FullLogin}. الرمز صالح لمدة 10 دقائق.",
            "otp", request.PlatformSchoolId, cancellationToken);
        return NoContent();
    }

    [HttpPost("confirmed")]
    public async Task<IActionResult> Confirm(
        [FromBody] ConfirmSchoolUserActivationRequest request,
        CancellationToken cancellationToken)
    {
        var configuredKey = configuration["InternalServices:ApiKey"];
        var suppliedKey = Request.Headers["X-Mdaresna-Internal-Key"].ToString();
        if (string.IsNullOrWhiteSpace(configuredKey) || !FixedEquals(configuredKey, suppliedKey))
            return Unauthorized();
        if (request.PlatformAccountId == Guid.Empty || request.PlatformSchoolId == Guid.Empty)
            return BadRequest();
        var school = await schools.FindByIdAsync(SchoolId.From(request.PlatformSchoolId), cancellationToken);
        if (school is null || school.Status != SchoolLifecycleStatus.Active) return NotFound();
        var identifier = await identityDb.LoginIdentifiers.Include(x => x.Account)
            .SingleOrDefaultAsync(x => x.AccountId == request.PlatformAccountId &&
                x.Type == LoginIdentifierType.Phone && x.SchoolId == null && x.IsPrimary,
                cancellationToken);
        if (identifier is null) return NotFound();
        if (identifier.Account.Status == AccountStatus.Active && identifier.IsVerified) return NoContent();
        if (identifier.Account.Status != AccountStatus.PendingVerification) return Conflict();
        var now = DateTimeOffset.UtcNow;
        identifier.IsVerified = true;
        identifier.VerifiedAtUtc = now;
        identifier.Account.Status = AccountStatus.Active;
        identifier.Account.UpdatedAtUtc = now;
        identityDb.SecurityEvents.Add(new IdentitySecurityEvent
        {
            Id = Guid.NewGuid(), AccountId = request.PlatformAccountId,
            EventType = "school.user.phone_verified", Succeeded = true, OccurredAtUtc = now
        });
        try { await identityDb.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return Conflict(); }
        return NoContent();
    }

    private static bool FixedEquals(string expected, string actual)
    {
        var a = Encoding.UTF8.GetBytes(expected); var b = Encoding.UTF8.GetBytes(actual);
        return a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b);
    }
}

public sealed record SendSchoolOwnerActivationCodeRequest(
    Guid PlatformAccountId,
    Guid PlatformSchoolId,
    [Required, MaxLength(140)] string FullLogin,
    [Required, RegularExpression("^[0-9]{8}$")] string Code);
public sealed record ConfirmSchoolUserActivationRequest(Guid PlatformAccountId, Guid PlatformSchoolId);
