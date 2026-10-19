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
        services.AddHostedService<SchoolDatabaseMigrationConsumerService>();
        services.AddScoped<SubmitSchoolRegistrationRequestHandler>();
        var routingCacheEnabled = configuration.GetValue<bool>("SchoolRoutingCache:Enabled");
        if (routingCacheEnabled)
        {
            var connection = configuration["SchoolRoutingCache:RedisConnection"];
            var ttl = configuration.GetValue<int?>("SchoolRoutingCache:TtlSeconds") ?? 2_592_000;
            if (string.IsNullOrWhiteSpace(connection) || ttl is < 1 or > 2_592_000)
                throw new InvalidOperationException("SchoolRoutingCache requires RedisConnection and TtlSeconds between 1 and 2592000 (30 days).");
            services.AddSingleton<ISchoolRoutingStore>(_ =>
            {
                var redis = StackExchange.Redis.ConfigurationOptions.Parse(connection);
                redis.AbortOnConnectFail = false;
                redis.ConnectTimeout = 500;
                redis.AsyncTimeout = 500;
                redis.ConnectRetry = 0;
                redis.BacklogPolicy = StackExchange.Redis.BacklogPolicy.FailFast;
                return new RedisSchoolRoutingStore(redis);
            });
        }
        services.AddSingleton(sp => new SchoolRoutingCache(
            routingCacheEnabled ? sp.GetRequiredService<ISchoolRoutingStore>() : null,
            configuration, sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SchoolRoutingCache>>()));
        services.AddHttpClient<ISchoolLoginTenantResolver, PlatformSchoolLoginTenantResolver>();
        services.AddHttpClient<SchoolRoutingSnapshotClient>();
        services.AddHttpClient<ISchoolRegistrationCatalog, PlatformSchoolRegistrationCatalog>();
        services.AddHostedService<SchoolRoutingCacheConsumerService>();
        services.AddHostedService<SchoolSubscriptionConsumerService>();
        services.AddHostedService<SchoolInvoiceConsumerService>();
        services.AddScoped<ISchoolLoginService, SchoolLocalLoginService>();
        services.AddScoped<ISchoolDbContextFactory, SchoolDbContextFactory>();
        services.AddHttpClient<ISchoolOwnerActivationCodeSender, PlatformSchoolOwnerActivationCodeSender>();
        services.AddHttpClient<ISchoolUserIdentityGateway, PlatformSchoolUserIdentityGateway>();
        services.AddHttpClient<IGlobalStudentGateway, PlatformGlobalStudentGateway>();
        services.AddScoped<ISchoolIdentityBootstrapper, SchoolIdentityBootstrapper>();
        services.AddScoped<SchoolOwnerActivationService>();
        services.AddScoped<IPasswordHasher<LocalUserAccount>, PasswordHasher<LocalUserAccount>>();
        services.AddSingleton<IClock>(SystemClock.Instance);
        return services;
    }
}
