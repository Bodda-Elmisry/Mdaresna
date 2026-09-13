using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Api.Errors;
using Mdaresna.Platform.Domain.Access;

namespace Mdaresna.Platform.Api.Controllers.Registry;

internal static class RegistryRequestContext
{
    public static IdentityAccountId Actor(HttpContext context)
    {
        if (context.User.FindFirst(PlatformTokenClaims.Purpose)?.Value !=
                PlatformTokenClaims.TokenPurpose ||
            !Guid.TryParse(
                context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,
                out var accountId) ||
            accountId == Guid.Empty)
        {
            // Every registry endpoint has a permission policy. Reaching this point
            // without a validated operator principal is a server-side invariant error.
            throw new InvalidOperationException("A validated platform operator is required.");
        }

        return IdentityAccountId.From(accountId);
    }

    public static Guid CorrelationId(HttpContext context)
    {
        if (Guid.TryParse(ApiResponseWriter.GetCorrelationId(context), out var correlationId) &&
            correlationId != Guid.Empty)
        {
            return correlationId;
        }

        throw new InvalidOperationException("Correlation middleware did not provide a valid ID.");
    }

    public static string? TraceParent => Activity.Current?.Id;

    public static void RequireId(Guid id, string field)
    {
        if (id == Guid.Empty)
        {
            throw new ValidationException($"{field} must not be empty.");
        }
    }

    public static void ValidateList(int pageNumber, int pageSize, string? search)
    {
        if (pageNumber < 1 || pageSize is < 1 or > 100 ||
            ((long)pageNumber - 1) * pageSize > int.MaxValue)
        {
            throw new ValidationException(
                "Page number must be positive and page size must be between 1 and 100.");
        }

        if (search?.Trim().Length > 200)
        {
            throw new ValidationException("Search must not exceed 200 characters.");
        }
    }
}
