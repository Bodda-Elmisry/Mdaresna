using Mdaresna.Platform.Infrastructure.DependencyInjection;
using Mdaresna.Platform.Worker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddSingleton<WorkerReadinessState>();
builder.Services.AddHostedService<PlatformWorkerService>();
builder.Services.AddHostedService<SchoolRegistrationRequestConsumerService>();
builder.Services.AddHostedService<PlatformOutboxPublisherService>();
builder.Services.AddHostedService<SchoolProvisioningResultConsumerService>();

builder.Services
    .AddHealthChecks()
    .AddCheck(
        "self",
        () => HealthCheckResult.Healthy(),
        tags: [HealthCheckTags.Live])
    .AddCheck<WorkerReadinessHealthCheck>(
        "worker-readiness",
        tags: [HealthCheckTags.Ready]);

builder.Services.AddPlatformInfrastructure(builder.Configuration);
builder.Services.AddPlatformNotificationDispatcher();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains(HealthCheckTags.Live)
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains(HealthCheckTags.Ready)
});

app.Run();

public partial class Program;
