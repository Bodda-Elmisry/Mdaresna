using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Hosting;
using Mdaresna.Platform.Api.Errors;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Api.Middleware;
using Mdaresna.Platform.Api.Realtime;
using Mdaresna.Platform.Infrastructure.DependencyInjection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = PlatformHostConfiguration.GetValidatedCorsOrigins(
    builder.Configuration,
    builder.Environment);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<PlatformExceptionHandler>();
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value!.Errors.Select(error =>
                        string.IsNullOrWhiteSpace(error.ErrorMessage)
                            ? "The value is invalid."
                            : error.ErrorMessage).ToArray());
            return new Microsoft.AspNetCore.Mvc.JsonResult(
                ApiResponse<object?>.Failure(400, "validation.failed", "Validation failed.", errors,
                    ApiResponseWriter.GetCorrelationId(context.HttpContext)))
            {
                StatusCode = 400
            };
        };
    });
builder.Services.AddSignalR();
builder.Services.AddHostedService<PlatformNotificationRealtimeDispatcher>();

builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = (context, cancellationToken) =>
        new ValueTask(ApiResponseWriter.WriteFailureAsync(
            context.HttpContext, 429, "rate_limit.exceeded", "Too many requests.",
            cancellationToken: cancellationToken));
    options.AddPolicy("platform-login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(PlatformCorsPolicy.Name, policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins);
        }

        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders(CorrelationIdMiddleware.HeaderName);
    });
});

builder.Services
    .AddHealthChecks()
    .AddCheck(
        "self",
        () => HealthCheckResult.Healthy(),
        tags: [HealthCheckTags.Live, HealthCheckTags.Ready]);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Mdaresna Platform API",
            Version = "v1",
            Description = "The control-plane API for the Mdaresna platform."
        });
    });
}

builder.Services.AddPlatformInfrastructure(builder.Configuration);
builder.Services.AddPlatformOperatorAuthentication(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();
app.UseCorrelationId();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseExceptionHandler();
app.UseStatusCodePages(async statusCodeContext =>
{
    var httpContext = statusCodeContext.HttpContext;
    if (!ApiResponseWriter.IsApplicationEndpoint(httpContext.Request.Path))
    {
        return;
    }

    var statusCode = httpContext.Response.StatusCode;
    var (code, message) = ApiResponseWriter.DescribeStatusCode(statusCode);
    await ApiResponseWriter.WriteFailureAsync(
        httpContext,
        statusCode,
        code,
        message,
        cancellationToken: httpContext.RequestAborted);
});
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors(PlatformCorsPolicy.Name);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "Mdaresna Platform API";
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Mdaresna Platform API v1");
    });
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", (HttpContext context) => ApiResponseWriter.ToResult(
    ApiResponse<object>.Success(
        new { service = "Mdaresna.Platform.Api", status = "running" },
        correlationId: ApiResponseWriter.GetCorrelationId(context))))
    .ExcludeFromDescription()
    .AllowAnonymous();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains(HealthCheckTags.Live)
}).AllowAnonymous();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains(HealthCheckTags.Ready)
}).AllowAnonymous();

app.MapControllers();
app.MapHub<PlatformNotificationHub>(PlatformNotificationHub.Path);

app.Run();

public partial class Program;
