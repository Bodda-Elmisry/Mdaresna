using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Api.Time;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration
    .GetSection("SchoolsHost:Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddSchoolsInfrastructure(builder.Configuration);
builder.Services.AddSchoolAuthentication(builder.Configuration);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<SchoolClock>();
builder.Services.AddCors(options => options.AddPolicy("SchoolsWeb", policy =>
{
    if (allowedOrigins.Length > 0) policy.WithOrigins(allowedOrigins);
    policy.AllowAnyHeader().AllowAnyMethod();
}));
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
app.UseCors("SchoolsWeb");
app.UseAuthentication();
app.UseAuthorization();
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
