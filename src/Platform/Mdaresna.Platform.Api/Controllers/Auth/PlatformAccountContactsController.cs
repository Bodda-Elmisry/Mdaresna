using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Errors;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Api.Controllers.Auth;

[ApiController]
[Authorize]
[Route("api/platform/v1/me/contacts")]
public sealed class PlatformAccountContactsController(IdentityDbContext identityDb) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var accountId = CurrentAccountId();
        var identifiers = await identityDb.LoginIdentifiers.AsNoTracking()
            .Where(x => x.AccountId == accountId && x.SchoolId == null &&
                (x.Type == LoginIdentifierType.Phone || x.Type == LoginIdentifierType.Email))
            .OrderBy(x => x.CreatedAtUtc)
            .ThenBy(x => x.Id)
            .Select(x => new { x.Id, x.Type, x.DisplayValue, x.IsVerified, x.IsPrimary })
            .ToListAsync(cancellationToken);
        var contacts = await identityDb.AccountContacts.AsNoTracking()
            .Where(x => x.AccountId == accountId)
            .OrderBy(x => x.CreatedAtUtc)
            .Select(x => new { x.Id, x.Type, x.Value })
            .ToListAsync(cancellationToken);

        // Existing identities are backfilled by migration; oldest is a fallback for
        // accounts imported later without an explicit primary marker.
        var primaryIds = identifiers.GroupBy(x => x.Type)
            .Select(group => group.FirstOrDefault(x => x.IsPrimary)?.Id ?? group.First().Id)
            .ToHashSet();
        var result = identifiers.Select(x => new ContactInformationResponse(
                x.Id, x.Type == LoginIdentifierType.Phone ? "phone" : "email",
                x.DisplayValue, x.IsVerified, primaryIds.Contains(x.Id), false))
            .Concat(contacts.Select(x => ToResponse(x.Id, x.Type, x.Value)))
            .OrderBy(x => x.Type == "phone" ? 0 : x.Type == "email" ? 1 : 2)
            .ThenByDescending(x => x.IsPrimary)
            .ToArray();

        return Ok(ApiResponse<ContactInformationResponse[]>.Success(
            result, correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] SaveContactInformationRequest request, CancellationToken cancellationToken)
    {
        if (!TryValidate(request, out var type, out var value, out var normalized))
            return InvalidContact();
        var accountId = CurrentAccountId();
        if (await IsDuplicateAsync(accountId, type, normalized, null, cancellationToken))
            return DuplicateContact();

        var now = DateTimeOffset.UtcNow;
        var contact = new AccountContact
        {
            Id = Guid.NewGuid(), AccountId = accountId, Type = type,
            Value = value, NormalizedValue = normalized,
            CreatedAtUtc = now, UpdatedAtUtc = now
        };
        identityDb.AccountContacts.Add(contact);
        try
        {
            await identityDb.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            if (await IsDuplicateAsync(accountId, type, normalized, contact.Id, cancellationToken))
                return DuplicateContact();
            throw;
        }
        return StatusCode(201, ApiResponse<ContactInformationResponse>.Success(
            ToResponse(contact.Id, contact.Type, contact.Value), statusCode: 201,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPut("{contactId:guid}")]
    public async Task<IActionResult> Update(
        Guid contactId, [FromBody] SaveContactInformationRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryValidate(request, out var type, out var value, out var normalized))
            return InvalidContact();
        var accountId = CurrentAccountId();
        var contact = await identityDb.AccountContacts.SingleOrDefaultAsync(
            x => x.Id == contactId && x.AccountId == accountId, cancellationToken);
        if (contact is null)
            return await MissingOrImmutableAsync(accountId, contactId, cancellationToken);
        if (await IsDuplicateAsync(accountId, type, normalized, contactId, cancellationToken))
            return DuplicateContact();

        contact.Type = type;
        contact.Value = value;
        contact.NormalizedValue = normalized;
        contact.UpdatedAtUtc = DateTimeOffset.UtcNow;
        try
        {
            await identityDb.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            if (await IsDuplicateAsync(accountId, type, normalized, contactId, cancellationToken))
                return DuplicateContact();
            throw;
        }
        return Ok(ApiResponse<ContactInformationResponse>.Success(
            ToResponse(contact.Id, contact.Type, contact.Value),
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpDelete("{contactId:guid}")]
    public async Task<IActionResult> Delete(Guid contactId, CancellationToken cancellationToken)
    {
        var accountId = CurrentAccountId();
        var contact = await identityDb.AccountContacts.SingleOrDefaultAsync(
            x => x.Id == contactId && x.AccountId == accountId, cancellationToken);
        if (contact is null)
            return await MissingOrImmutableAsync(accountId, contactId, cancellationToken);

        identityDb.AccountContacts.Remove(contact);
        await identityDb.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<object?>.Success(null,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    private async Task<bool> IsDuplicateAsync(Guid accountId, AccountContactType type,
        string normalized, Guid? excludeId, CancellationToken cancellationToken)
    {
        var duplicate = await identityDb.AccountContacts.AsNoTracking().AnyAsync(x =>
            x.AccountId == accountId && x.Type == type && x.NormalizedValue == normalized &&
            (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
        if (duplicate || type == AccountContactType.Address)
            return duplicate;
        var identifierType = type == AccountContactType.Phone
            ? LoginIdentifierType.Phone : LoginIdentifierType.Email;
        return await identityDb.LoginIdentifiers.AsNoTracking().AnyAsync(x =>
            x.AccountId == accountId && x.SchoolId == null &&
            x.Type == identifierType && x.NormalizedValue == normalized,
            cancellationToken);
    }

    private async Task<IActionResult> MissingOrImmutableAsync(
        Guid accountId, Guid contactId, CancellationToken cancellationToken)
    {
        var isIdentifier = await identityDb.LoginIdentifiers.AsNoTracking().AnyAsync(x =>
            x.Id == contactId && x.AccountId == accountId && x.SchoolId == null &&
            (x.Type == LoginIdentifierType.Phone || x.Type == LoginIdentifierType.Email),
            cancellationToken);
        return isIdentifier
            ? StatusCode(403, ApiResponse<object?>.Failure(403, "contact.immutable",
                "Account login contacts cannot be changed here.",
                correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)))
            : NotFound(ApiResponse<object?>.Failure(404, "contact.not_found",
                "Contact was not found.",
                correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    private IActionResult InvalidContact() => BadRequest(ApiResponse<object?>.Failure(
        400, "contact.invalid", "Enter a valid phone, email, or address.",
        correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));

    private IActionResult DuplicateContact() => Conflict(ApiResponse<object?>.Failure(
        409, "contact.duplicate", "This contact already exists on the account.",
        correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));

    private Guid CurrentAccountId() =>
        Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) && id != Guid.Empty
            ? id : throw new InvalidOperationException("A validated Platform account is required.");

    private static ContactInformationResponse ToResponse(Guid id, AccountContactType type, string value) =>
        new(id, type switch
        {
            AccountContactType.Phone => "phone",
            AccountContactType.Email => "email",
            _ => "address"
        }, value, type == AccountContactType.Address ? null : false, false, true);

    private static bool TryValidate(SaveContactInformationRequest request,
        out AccountContactType type, out string value, out string normalized)
    {
        type = request.Type?.Trim().ToLowerInvariant() switch
        {
            "phone" => AccountContactType.Phone,
            "email" => AccountContactType.Email,
            "address" => AccountContactType.Address,
            _ => default
        };
        value = request.Value?.Trim() ?? string.Empty;
        normalized = string.Empty;
        if (type == default || value.Length == 0 || value.Length > 500)
            return false;

        switch (type)
        {
            case AccountContactType.Phone:
                if (!Regex.IsMatch(value, @"^\+?[0-9]{8,16}$")) return false;
                normalized = value;
                break;
            case AccountContactType.Email:
                if (value.Length > 320 || !MailAddress.TryCreate(value, out var address) ||
                    !address.Address.Equals(value, StringComparison.OrdinalIgnoreCase)) return false;
                normalized = value.ToUpperInvariant();
                break;
            case AccountContactType.Address:
                normalized = value;
                break;
            default:
                return false;
        }
        return true;
    }
}

public sealed record SaveContactInformationRequest([Required] string Type, [Required] string Value);

public sealed record ContactInformationResponse(
    Guid Id, string Type, string Value, bool? IsVerified, bool IsPrimary, bool CanEdit);
