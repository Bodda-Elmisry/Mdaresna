using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/schools/v1/me/contacts")]
public sealed class SchoolAccountContactsController(
    ISchoolDbContextFactory dbFactory, ISchoolUserIdentityGateway identityGateway) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        await using var db = await RequireDb(cancellationToken);
        if (db is null) return Unauthorized();
        var personId = CurrentPersonId();
        var primary = await GetPrimaryContactAsync(db, personId, cancellationToken);
        if (primary is null) return IdentityUnavailable();
        var contacts = await db.PersonContacts.AsNoTracking()
            .Where(x => x.PersonId == personId && !x.IsPrimary)
            .OrderBy(x => x.Type).ThenBy(x => x.CreatedAtUtc)
            .Select(x => new SchoolContactInformationResponse(x.Id,
                x.Type == PersonContactType.Phone ? "phone" : x.Type == PersonContactType.Email ? "email" : "address",
                x.Value, x.Type == PersonContactType.Address ? null : x.IsVerified, false, true))
            .ToListAsync(cancellationToken);
        contacts.Insert(0, new(primary.Id, "phone", primary.PrimaryPhone, primary.PhoneVerified, true, false));
        return Ok(ApiResponse<SchoolContactInformationResponse[]>.Success(contacts.ToArray(),
            correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveSchoolContactInformationRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryValidate(request, out var type, out var value, out var normalized)) return InvalidContact();
        await using var db = await RequireDb(cancellationToken);
        if (db is null) return Unauthorized();
        var personId = CurrentPersonId();
        if (!await db.Persons.AsNoTracking().AnyAsync(x => x.Id == personId, cancellationToken)) return NotFoundContact();
        var primary = type == PersonContactType.Phone
            ? await GetPrimaryContactAsync(db, personId, cancellationToken) : null;
        if (type == PersonContactType.Phone && primary is null) return IdentityUnavailable();
        if (await IsDuplicateAsync(db, personId, type, normalized, null, primary, cancellationToken)) return DuplicateContact();

        var now = DateTimeOffset.UtcNow;
        var contact = new PersonContact { Id = Guid.NewGuid(), PersonId = personId, Type = type, Value = value,
            NormalizedValue = normalized, IsPrimary = false, IsVerified = false, CreatedAtUtc = now, UpdatedAtUtc = now };
        db.PersonContacts.Add(contact);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException)
        {
            if (await IsDuplicateAsync(db, personId, type, normalized, contact.Id, primary, cancellationToken)) return DuplicateContact();
            throw;
        }
        return StatusCode(201, ApiResponse<SchoolContactInformationResponse>.Success(ToResponse(contact),
            statusCode: 201, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut("{contactId:guid}")]
    public async Task<IActionResult> Update(Guid contactId, [FromBody] SaveSchoolContactInformationRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryValidate(request, out var type, out var value, out var normalized)) return InvalidContact();
        await using var db = await RequireDb(cancellationToken);
        if (db is null) return Unauthorized();
        var personId = CurrentPersonId();
        var contact = await db.PersonContacts.SingleOrDefaultAsync(
            x => x.Id == contactId && x.PersonId == personId, cancellationToken);
        SchoolUserPrimaryContact? primary = null;
        if (contact is null)
        {
            primary = await GetPrimaryContactAsync(db, personId, cancellationToken);
            return primary?.Id == contactId ? ImmutableContact() : NotFoundContact();
        }
        if (contact.IsPrimary) return ImmutableContact();
        primary = type == PersonContactType.Phone
            ? await GetPrimaryContactAsync(db, personId, cancellationToken) : null;
        if (type == PersonContactType.Phone && primary is null) return IdentityUnavailable();
        if (await IsDuplicateAsync(db, personId, type, normalized, contactId, primary, cancellationToken)) return DuplicateContact();

        contact.Type = type; contact.Value = value; contact.NormalizedValue = normalized;
        contact.IsVerified = false; contact.UpdatedAtUtc = DateTimeOffset.UtcNow;
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException)
        {
            if (await IsDuplicateAsync(db, personId, type, normalized, contactId, primary, cancellationToken)) return DuplicateContact();
            throw;
        }
        return Ok(ApiResponse<SchoolContactInformationResponse>.Success(ToResponse(contact),
            correlationId: HttpContext.TraceIdentifier));
    }

    [HttpDelete("{contactId:guid}")]
    public async Task<IActionResult> Delete(Guid contactId, CancellationToken cancellationToken)
    {
        await using var db = await RequireDb(cancellationToken);
        if (db is null) return Unauthorized();
        var contact = await db.PersonContacts.SingleOrDefaultAsync(
            x => x.Id == contactId && x.PersonId == CurrentPersonId(), cancellationToken);
        if (contact is null)
        {
            var primary = await GetPrimaryContactAsync(db, CurrentPersonId(), cancellationToken);
            return primary?.Id == contactId ? ImmutableContact() : NotFoundContact();
        }
        if (contact.IsPrimary) return ImmutableContact();
        db.PersonContacts.Remove(contact);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    private static Task<bool> IsDuplicateAsync(SchoolsDbContext db, Guid personId, PersonContactType type,
        string normalized, Guid? excludeId, SchoolUserPrimaryContact? primary,
        CancellationToken cancellationToken) =>
        type == PersonContactType.Phone && primary?.PrimaryPhone == normalized
            ? Task.FromResult(true)
            : db.PersonContacts.AsNoTracking().AnyAsync(x => x.PersonId == personId && x.Type == type &&
                x.NormalizedValue == normalized && (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

    private async Task<SchoolUserPrimaryContact?> GetPrimaryContactAsync(SchoolsDbContext db, Guid personId,
        CancellationToken cancellationToken)
    {
        var platformAccountId = await db.LocalUsers.AsNoTracking().Where(x => x.PersonId == personId)
            .Select(x => x.PlatformAccountId).SingleOrDefaultAsync(cancellationToken);
        var platformSchoolId = await db.SchoolInformation.AsNoTracking()
            .Select(x => (Guid?)x.PlatformSchoolReferenceId).SingleOrDefaultAsync(cancellationToken);
        return platformAccountId.HasValue && platformSchoolId.HasValue
            ? await identityGateway.GetPrimaryContactAsync(platformSchoolId.Value, platformAccountId.Value, cancellationToken)
            : null;
    }

    private Task<SchoolsDbContext?> RequireDb(CancellationToken cancellationToken) =>
        dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty, cancellationToken);
    private Guid CurrentPersonId() => Guid.TryParse(User.FindFirst(SchoolClaimTypes.PersonId)?.Value, out var id) && id != Guid.Empty
        ? id : throw new InvalidOperationException("A validated school person is required.");
    private IActionResult InvalidContact() => BadRequest(ApiResponse<object?>.Failure(400, "contact.invalid",
        "Enter a valid phone, email, or address.", correlationId: HttpContext.TraceIdentifier));
    private IActionResult DuplicateContact() => Conflict(ApiResponse<object?>.Failure(409, "contact.duplicate",
        "This contact already exists.", correlationId: HttpContext.TraceIdentifier));
    private IActionResult ImmutableContact() => StatusCode(403, ApiResponse<object?>.Failure(403, "contact.immutable",
        "The primary contact cannot be changed here.", correlationId: HttpContext.TraceIdentifier));
    private IActionResult NotFoundContact() => NotFound(ApiResponse<object?>.Failure(404, "contact.not_found",
        "Contact was not found.", correlationId: HttpContext.TraceIdentifier));
    private IActionResult IdentityUnavailable() => StatusCode(503, ApiResponse<object?>.Failure(503,
        "contact.identity_unavailable", "The primary Identity contact is unavailable.",
        correlationId: HttpContext.TraceIdentifier));

    private static SchoolContactInformationResponse ToResponse(PersonContact contact) => new(contact.Id,
        contact.Type == PersonContactType.Phone ? "phone" : contact.Type == PersonContactType.Email ? "email" : "address",
        contact.Value, contact.Type == PersonContactType.Address ? null : contact.IsVerified,
        contact.IsPrimary, !contact.IsPrimary);

    private static bool TryValidate(SaveSchoolContactInformationRequest request, out PersonContactType type,
        out string value, out string normalized)
    {
        type = request.Type?.Trim().ToLowerInvariant() switch
        {
            "phone" => PersonContactType.Phone, "email" => PersonContactType.Email,
            "address" => PersonContactType.Address, _ => default
        };
        value = request.Value?.Trim() ?? string.Empty;
        normalized = string.Empty;
        if (type == default || value.Length == 0 || value.Length > 500) return false;
        switch (type)
        {
            case PersonContactType.Phone:
                if (!Regex.IsMatch(value, @"^\+?[0-9]{8,16}$")) return false;
                normalized = value;
                break;
            case PersonContactType.Email:
                if (value.Length > 320 || !MailAddress.TryCreate(value, out var address) ||
                    !address.Address.Equals(value, StringComparison.OrdinalIgnoreCase)) return false;
                normalized = value.ToUpperInvariant();
                break;
            case PersonContactType.Address:
                normalized = value;
                break;
            default: return false;
        }
        return true;
    }
}

public sealed record SaveSchoolContactInformationRequest([Required] string Type, [Required] string Value);
public sealed record SchoolContactInformationResponse(Guid Id, string Type, string Value, bool? IsVerified,
    bool IsPrimary, bool CanEdit);
