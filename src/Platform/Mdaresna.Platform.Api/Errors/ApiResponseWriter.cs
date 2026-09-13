using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Middleware;

namespace Mdaresna.Platform.Api.Errors;

internal static class ApiResponseWriter
{
    public static IResult ToResult<T>(ApiResponse<T> response)
    {
        ArgumentNullException.ThrowIfNull(response);
        return Results.Json((object)response, statusCode: response.StatusCode);
    }

    public static bool IsApplicationEndpoint(PathString path) =>
        !path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase) &&
        !path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase);

    public static string? GetCorrelationId(HttpContext context) =>
        context.Items.TryGetValue(CorrelationIdMiddleware.HttpContextItemKey, out var value)
            ? value as string
            : null;

    public static (string Code, string Message) DescribeStatusCode(int statusCode) =>
        statusCode switch
        {
            StatusCodes.Status400BadRequest => ("request.invalid", "The request is invalid."),
            StatusCodes.Status401Unauthorized => ("auth.required", "Authentication is required."),
            StatusCodes.Status403Forbidden => ("access.denied", "Access is denied."),
            StatusCodes.Status404NotFound => ("resource.not_found", "The resource was not found."),
            StatusCodes.Status405MethodNotAllowed => ("method.not_allowed", "The HTTP method is not allowed."),
            StatusCodes.Status413PayloadTooLarge => ("request.too_large", "The request is too large."),
            StatusCodes.Status415UnsupportedMediaType => ("request.unsupported_media_type", "The media type is not supported."),
            StatusCodes.Status429TooManyRequests => ("rate_limit.exceeded", "Too many requests."),
            _ when statusCode >= 500 => ("server.unexpected", "An unexpected error occurred."),
            _ => ("http.error", "The request could not be completed.")
        };

    public static Task WriteFailureAsync(
        HttpContext context,
        int statusCode,
        string code,
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null,
        CancellationToken cancellationToken = default)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(
            ApiResponse<object?>.Failure(
                statusCode,
                code,
                message,
                errors,
                GetCorrelationId(context)),
            cancellationToken);
    }
}
