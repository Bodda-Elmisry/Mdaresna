using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mdaresna.Schools.Application.Registration;
using Mdaresna.Schools.Infrastructure.Messaging;
using Mdaresna.Schools.Application.Identity;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Mdaresna.SharedKernel.Time;

namespace Mdaresna.Schools.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers school infrastructure that is safe before a school database has been selected.
    /// Per-school DbContext registration belongs to the database-routing phase.
    /// </summary>
    public static IServiceCollection AddSchoolsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddScoped<ISchoolRegistrationRequestPublisher, RabbitMqSchoolRegistrationRequestPublisher>();
        services.AddHostedService<SchoolProvisioningConsumerService>();
        services.AddScoped<SubmitSchoolRegistrationRequestHandler>();
        services.AddHttpClient<ISchoolLoginTenantResolver, PlatformSchoolLoginTenantResolver>();
        services.AddScoped<ISchoolLoginService, SchoolLocalLoginService>();
        services.AddScoped<ISchoolDbContextFactory, SchoolDbContextFactory>();
        services.AddScoped<ISchoolIdentityBootstrapper, SchoolIdentityBootstrapper>();
        services.AddScoped<SchoolOwnerActivationService>();
        services.AddScoped<IPasswordHasher<LocalUserAccount>, PasswordHasher<LocalUserAccount>>();
        services.AddSingleton<IClock>(SystemClock.Instance);
        return services;
    }
}
