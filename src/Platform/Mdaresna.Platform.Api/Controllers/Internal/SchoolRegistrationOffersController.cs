using System.Security.Cryptography;
using System.Text;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Application.Billing.Subscriptions;
using Mdaresna.Schools.Contracts.Registration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Mdaresna.Platform.Api.Controllers.Internal;

[ApiController, AllowAnonymous, PlatformInternalService, Route("api/platform/v1/internal/school-registration-offers")]
public sealed class SchoolRegistrationOffersController(IPlatformInvoiceService invoices, IConfiguration config) : ControllerBase
{
    private bool Allowed()
    {
        var expected = config["InternalServices:ApiKey"]; var supplied = Request.Headers["X-Mdaresna-Internal-Key"].ToString();
        if (string.IsNullOrWhiteSpace(expected)) return false;
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(supplied));
    }
    [HttpGet("plans")]
    public async Task<IActionResult> Plans(CancellationToken ct) => Allowed() ? Ok(await invoices.RegistrationPlansAsync(ct)) : Unauthorized();
    [HttpPost]
    public async Task<IActionResult> Prepare(PrepareRegistrationOffer input, CancellationToken ct) => Allowed() ? Ok(new { offerId = await invoices.PrepareRegistrationAsync(input, ct) }) : Unauthorized();
}
