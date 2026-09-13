using System.Diagnostics;

namespace Mdaresna.Platform.Api.Middleware;

public sealed class CorrelationIdMiddleware(
    RequestDelegate next,
    ILogger<CorrelationIdMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-ID";
    public const string HttpContextItemKey = "Mdaresna.CorrelationId";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetCorrelationId(context.Request.Headers);

        context.Items[HttpContextItemKey] = correlationId;
        Activity.Current?.SetTag("correlation.id", correlationId);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            await next(context);
        }
    }

    private static string GetCorrelationId(IHeaderDictionary headers)
    {
        if (headers.TryGetValue(HeaderName, out var values) &&
            values.Count == 1 &&
            Guid.TryParse(values[0], out var suppliedCorrelationId) &&
            suppliedCorrelationId != Guid.Empty)
        {
            return suppliedCorrelationId.ToString("N");
        }

        return Guid.NewGuid().ToString("N");
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app) =>
        app.UseMiddleware<CorrelationIdMiddleware>();
}
