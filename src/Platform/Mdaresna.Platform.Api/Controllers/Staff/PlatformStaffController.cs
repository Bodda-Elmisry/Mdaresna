using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Api.Errors;
using Mdaresna.Platform.Application.Access.Staff;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Platform.Api.Controllers.Staff;

[ApiController]
[Route("api/platform/v1/staff")]
[PlatformPermission("platform.access.manage")]
public sealed class PlatformStaffController(
    IPlatformStaffDirectory directory,
    IPlatformStaffRoleManager roles,
    IPlatformRoleCatalog roleCatalog,
    IPlatformStaffManagement management,
    IdentityDbContext identityDb,
    PlatformDbContext platformDb) : ControllerBase
{
    [HttpGet("roles")]
    public async Task<IActionResult> ListRoles(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var catalog = await roleCatalog.ListAsync(includeInactive, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PlatformRoleCatalogItem>>.Success(
            catalog, correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] Guid? roleId = null,
        CancellationToken cancellationToken = default)
    {
        var page = await directory.ListAsync(pageNumber, pageSize, cancellationToken, search, roleId);
        return Ok(PagedApiResponse<PlatformStaffDirectoryItem>.Success(
            page.Items, page.TotalCount, page.PageNumber, page.PageSize,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpGet("{accountId:guid}/profile")]
    public async Task<IActionResult> Profile(Guid accountId, CancellationToken cancellationToken)
    {
        var isStaff = await platformDb.LocalUsers.AsNoTracking()
            .AnyAsync(user => user.PersonId == accountId, cancellationToken);
        if (!isStaff)
            return NotFound(ApiResponse<object?>.Failure(404, "staff.not_found", "Staff member was not found.",
                correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));

        var account = await identityDb.Accounts.AsNoTracking()
            .Where(person => person.Id == accountId)
            .Select(person => new { person.DisplayName, person.DateOfBirth, person.GenderCode })
            .SingleOrDefaultAsync(cancellationToken);
        if (account is null)
            return NotFound(ApiResponse<object?>.Failure(404, "staff.profile_not_found", "Person profile was not found.",
                correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));

        var identifiers = await identityDb.LoginIdentifiers.AsNoTracking()
            .Where(identifier => identifier.AccountId == accountId && identifier.SchoolId == null &&
                (identifier.Type == LoginIdentifierType.Phone || identifier.Type == LoginIdentifierType.Email))
            .OrderBy(identifier => identifier.CreatedAtUtc).ThenBy(identifier => identifier.Id)
            .Select(identifier => new { identifier.Id, identifier.Type, identifier.DisplayValue,
                identifier.IsVerified, identifier.IsPrimary })
            .ToArrayAsync(cancellationToken);
        var primaryIds = identifiers.GroupBy(identifier => identifier.Type)
            .Select(group => group.FirstOrDefault(identifier => identifier.IsPrimary)?.Id ?? group.First().Id)
            .ToHashSet();
        var loginContacts = identifiers.Select(identifier => new StaffProfileContact(
            identifier.Type == LoginIdentifierType.Phone ? "phone" : "email",
            identifier.DisplayValue, identifier.IsVerified, primaryIds.Contains(identifier.Id)));
        var supplementaryContacts = await identityDb.AccountContacts.AsNoTracking()
            .Where(contact => contact.AccountId == accountId)
            .OrderBy(contact => contact.CreatedAtUtc).ThenBy(contact => contact.Id)
            .Select(contact => new { contact.Type, contact.Value })
            .ToArrayAsync(cancellationToken);
        var contacts = loginContacts.Concat(supplementaryContacts.Select(contact => new StaffProfileContact(
                contact.Type == AccountContactType.Phone ? "phone" :
                contact.Type == AccountContactType.Email ? "email" : "address",
                contact.Value, false, false)))
            .OrderBy(contact => contact.Type == "phone" ? 0 : contact.Type == "email" ? 1 : 2)
            .ThenByDescending(contact => contact.IsPrimary)
            .ToArray();
        var hasImage = await identityDb.AccountProfileImages.AsNoTracking()
            .AnyAsync(image => image.AccountId == accountId, cancellationToken);

        return Ok(ApiResponse<StaffProfileResponse>.Success(
            new(account.DisplayName, account.DateOfBirth, account.GenderCode, hasImage, contacts),
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpGet("lookup")]
    public async Task<IActionResult> Lookup([FromQuery] string phone, CancellationToken cancellationToken)
    {
        var result = await management.LookupAsync(CurrentAccountId(), phone, cancellationToken);
        return Ok(ApiResponse<PlatformStaffLookupResult>.Success(result,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPost("invitations")]
    public async Task<IActionResult> Invite([FromBody] InvitePlatformStaffRequest request,
        CancellationToken cancellationToken)
    {
        var accountId = await management.InviteAsync(CurrentAccountId(), request, cancellationToken,
            ApiResponseWriter.GetCorrelationId(HttpContext));
        return StatusCode(201, ApiResponse<object>.Success(new { accountId }, statusCode: 201,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPut("{accountId:guid}")]
    public async Task<IActionResult> Update(Guid accountId, [FromBody] UpdatePlatformStaffRequest request,
        CancellationToken cancellationToken)
    {
        await management.UpdateAsync(CurrentAccountId(), accountId, request, cancellationToken,
            ApiResponseWriter.GetCorrelationId(HttpContext));
        return Ok(ApiResponse<object?>.Success(null,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPut("{accountId:guid}/active")]
    public async Task<IActionResult> SetActive(Guid accountId, [FromBody] SetStaffActiveRequest request,
        CancellationToken cancellationToken)
    {
        await management.SetActiveAsync(CurrentAccountId(), accountId, request.IsActive, cancellationToken,
            ApiResponseWriter.GetCorrelationId(HttpContext));
        return Ok(ApiResponse<object?>.Success(null,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpGet("{accountId:guid}/image")]
    public async Task<IActionResult> Image(Guid accountId, CancellationToken cancellationToken)
    {
        if (!await platformDb.LocalUsers.AsNoTracking().AnyAsync(x => x.PersonId == accountId, cancellationToken) ||
            !await identityDb.AccountProfileImages.AsNoTracking().AnyAsync(x => x.AccountId == accountId, cancellationToken))
            return NotFound();
        var image = await identityDb.AccountProfileImages.AsNoTracking()
            .Where(x => x.AccountId == accountId)
            .Select(x => new { x.Content, x.ContentType })
            .SingleAsync(cancellationToken);
        Response.Headers.CacheControl = "private, no-store";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(image.Content, image.ContentType);
    }

    [HttpPost("{accountId:guid}/roles")]
    public async Task<IActionResult> AssignRole(
        Guid accountId,
        [FromBody] AssignStaffRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await roles.AssignAsync(
            CurrentAccountId(), accountId, request.RoleId, cancellationToken,
            ApiResponseWriter.GetCorrelationId(HttpContext));
        var response = ApiResponse<PlatformStaffRoleAssignmentResult>.Success(
            result,
            statusCode: result.Changed ? 201 : 200,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext));
        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("roles/{assignmentId:guid}")]
    public async Task<IActionResult> RevokeRole(
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        var result = await roles.RevokeAsync(
            CurrentAccountId(), assignmentId, cancellationToken,
            ApiResponseWriter.GetCorrelationId(HttpContext));
        return Ok(ApiResponse<PlatformStaffRoleAssignmentResult>.Success(
            result,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    private Guid CurrentAccountId() =>
        Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var accountId) &&
        accountId != Guid.Empty
            ? accountId
            : throw new InvalidOperationException("A validated Platform account is required.");
}

public sealed record AssignStaffRoleRequest(Guid RoleId);
public sealed record SetStaffActiveRequest(bool IsActive);
public sealed record StaffProfileContact(string Type, string Value, bool IsVerified, bool IsPrimary);
public sealed record StaffProfileResponse(string? DisplayName, DateOnly? DateOfBirth, string? GenderCode,
    bool HasImage, IReadOnlyList<StaffProfileContact> Contacts);
