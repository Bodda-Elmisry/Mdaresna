using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Application.Identity;
using Microsoft.AspNetCore.Diagnostics;

namespace Mdaresna.Schools.Api.Errors;

public sealed class SchoolRoutingExceptionHandler(ILogger<SchoolRoutingExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not SchoolRoutingUnavailableException || context.Response.HasStarted ||
            context.RequestAborted.IsCancellationRequested) return false;

        // Do not log routing credentials, request headers, or upstream response bodies.
        logger.LogWarning("School routing unavailable. CorrelationId: {CorrelationId}", context.TraceIdentifier);
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.Headers.RetryAfter = "5";
        await context.Response.WriteAsJsonAsync(ApiResponse<object?>.Failure(503,
            "auth.school_routing_unavailable", "School access is temporarily unavailable. Try again shortly.",
            correlationId: context.TraceIdentifier), ct);
        return true;
    }
}
