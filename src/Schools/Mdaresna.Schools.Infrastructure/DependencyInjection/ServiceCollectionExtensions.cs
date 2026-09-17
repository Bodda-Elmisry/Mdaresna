using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mdaresna.Schools.Application.Registration;
using Mdaresna.Schools.Infrastructure.Messaging;
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
        services.AddScoped<SubmitSchoolRegistrationRequestHandler>();
        services.AddSingleton<IClock>(SystemClock.Instance);
        return services;
    }
}
