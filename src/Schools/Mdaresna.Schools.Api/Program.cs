using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddSchoolsInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks().AddCheck(
    "self",
    () => HealthCheckResult.Healthy(),
    tags: ["live", "ready"]);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mdaresna Schools API",
        Version = "v1",
        Description = "The tenant-scoped operational API for Mdaresna schools."
    }));
}

var app = builder.Build();

if (!app.Environment.IsDevelopment()) app.UseHsts();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", (HttpContext context) => ApiResponse<object>.Success(
        new { service = "Mdaresna.Schools.Api", status = "running" },
        correlationId: context.TraceIdentifier))
    .AllowAnonymous()
    .ExcludeFromDescription();
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live")
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready")
}).AllowAnonymous();
app.MapControllers();

app.Run();

public partial class Program;
