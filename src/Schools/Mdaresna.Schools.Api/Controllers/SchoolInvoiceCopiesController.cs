using System.Text.Json;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Contracts.Provisioning;
using Mdaresna.Schools.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController, Authorize, Route("api/schools/v1/subscription-invoices")]
public sealed class SchoolInvoiceCopiesController(ISchoolDbContextFactory factory) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, CancellationToken ct = default)
    {
        if (!User.HasClaim(SchoolClaimTypes.Permission, "school.profile.manage")) return Forbid();
        if (page is < 1 or > 100000) return BadRequest();
        await using var db = await factory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? "", ct);
        if (db == null) return Unauthorized();
        var rows = await db.InvoiceCopies.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc).ThenBy(x => x.Id)
            .Skip((page - 1) * 30).Take(31).ToArrayAsync(ct);
        return Ok(ApiResponse<object>.Success(new { hasMore = rows.Length > 30, items = rows.Take(30).Select(x => new {
            invoice = JsonSerializer.Deserialize<SchoolInvoiceSnapshotChangedV1>(x.PayloadJson), x.SyncedAtUtc }) }));
    }
}
