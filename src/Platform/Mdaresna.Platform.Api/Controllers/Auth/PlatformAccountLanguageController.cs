using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Errors;
using Mdaresna.Platform.Infrastructure.IdentityAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Platform.Api.Controllers.Auth;

[ApiController]
[Authorize]
[Route("api/platform/v1/me/language")]
public sealed class PlatformAccountLanguageController(AccountAppLanguageService languages) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var stored = await languages.GetStoredAsync(
            CurrentAccountId(), AccountAppLanguageService.PlatformApp, cancellationToken);
        return Ok(ApiResponse<AccountLanguageResponse>.Success(
            new AccountLanguageResponse(stored ?? AccountAppLanguageService.DefaultLanguage,
                stored is not null),
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPut]
    public async Task<IActionResult> Put(
        [FromBody] SetAccountLanguageRequest request, CancellationToken cancellationToken)
    {
        if (!AccountAppLanguageService.IsSupported(request.LanguageCode))
            return BadRequest(ApiResponse<object?>.Failure(
                400, "language.unsupported", "Language must be 'ar' or 'en'.",
                correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));

        var code = await languages.SetAsync(CurrentAccountId(),
            AccountAppLanguageService.PlatformApp, request.LanguageCode, cancellationToken);
        return Ok(ApiResponse<AccountLanguageResponse>.Success(
            new AccountLanguageResponse(code, true),
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    private Guid CurrentAccountId() =>
        Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) && id != Guid.Empty
            ? id
            : throw new InvalidOperationException("A validated Platform account is required.");
}

public sealed record SetAccountLanguageRequest([Required] string LanguageCode);
public sealed record AccountLanguageResponse(string LanguageCode, bool IsStored);
