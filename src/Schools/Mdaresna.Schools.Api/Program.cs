using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Api.Time;
using Mdaresna.Schools.Api.Documents;

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
builder.Services.AddSingleton<ISchoolDocumentStorage, LocalSchoolDocumentStorage>();
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
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Mdaresna Schools API",
            Version = "v1",
            Description = "The tenant-scoped operational API for Mdaresna schools."
        });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter the school access token."
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }] = []
        });
    });
}

var app = builder.Build();

if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseCors("SchoolsWeb");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "Mdaresna Schools API";
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Mdaresna Schools API v1");
    });
}
app.UseAuthentication();
app.UseAuthorization();

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
