using Mdaresna.Platform.Application.Abstractions.Messaging;
using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Application.Registry.BeginSchoolProvisioning;
using Mdaresna.Platform.Application.Registry.CreateTenant;
using Mdaresna.Platform.Application.Registry.RegisterSchool;
using Mdaresna.Platform.Application.Registry.Lifecycle;
using Mdaresna.Platform.Application.Registry.Read;
using Mdaresna.Platform.Application.Billing;
using Mdaresna.Platform.Application.Billing.SubmitSchoolPlatformPayment;
using Mdaresna.Platform.Application.Billing.ReviewSchoolPlatformPayment;
using Mdaresna.Platform.Application.Billing.Read;
using Mdaresna.Platform.Application.Billing.Units;
using Mdaresna.Platform.Application.Access.Staff;
using Mdaresna.Platform.Infrastructure.Messaging;
using Mdaresna.Platform.Infrastructure.HealthChecks;
using Mdaresna.Platform.Infrastructure.Persistence;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Repositories;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Registry;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;
using Mdaresna.Platform.Infrastructure.IdentityAuth.Staff;
using Mdaresna.Platform.Infrastructure.Security;
using Mdaresna.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Mdaresna.Platform.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public const string PlatformConnectionName = "PlatformConnection";
    public const string IdentityConnectionName = "IdentityConnection";

    public static IServiceCollection AddPlatformInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var platformConnection = GetRequiredConnectionString(configuration, PlatformConnectionName);
        var identityConnection = GetRequiredConnectionString(configuration, IdentityConnectionName);
        var provider = configuration["PlatformDatabase:Provider"] ?? "SqlServer";
        if (provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
        {
            PostgreSqlDatabaseTargetValidator.EnsureDifferent(platformConnection, identityConnection);
            services.AddDbContext<PostgreSqlPlatformDbContext>(options =>
                ConfigurePostgreSql(options, platformConnection, "platform"));
            services.AddDbContext<PostgreSqlIdentityDbContext>(options =>
                ConfigurePostgreSql(options, identityConnection, "identity"));
            services.AddScoped<PlatformDbContext>(sp => sp.GetRequiredService<PostgreSqlPlatformDbContext>());
            services.AddScoped<IdentityDbContext>(sp => sp.GetRequiredService<PostgreSqlIdentityDbContext>());
        }
        else if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            SqlDatabaseTargetValidator.EnsureDifferent(platformConnection, identityConnection);
            services.AddDbContext<PlatformDbContext>(options =>
                ConfigureSqlServer(options, platformConnection, "platform"));
            services.AddDbContext<IdentityDbContext>(options =>
                ConfigureSqlServer(options, identityConnection, "identity"));
        }
        else
        {
            throw new InvalidOperationException("PlatformDatabase:Provider must be SqlServer or PostgreSql.");
        }

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ISchoolRegistrationRepository, SchoolRegistrationRepository>();
        services.AddScoped<IPlatformRoleRepository, PlatformRoleRepository>();
        services.AddScoped<IPlatformRoleAssignmentRepository, PlatformRoleAssignmentRepository>();
        services.AddScoped<IPlatformUnitOfWork, PlatformUnitOfWork>();
        services.AddScoped<IPlatformPermissionEvaluator, PlatformPermissionEvaluator>();
        services.AddScoped<IPlatformOutboxWriter, PlatformOutboxWriter>();
        services.AddScoped<IPlatformSmsSender, PlatformDbSmsSender>();
        services.AddScoped<PlatformSmsLogEventIngestor>();
        services.AddScoped<PlatformSmsSecretProtector>();
        services.AddScoped<PlatformSmsProviderService>();
        services.Configure<PlatformFirebaseOptions>(options =>
        {
            options.Enabled = bool.TryParse(
                configuration[$"{PlatformFirebaseOptions.SectionName}:Enabled"], out var enabled) && enabled;
            options.ProjectId = configuration[$"{PlatformFirebaseOptions.SectionName}:ProjectId"]?.Trim()
                                ?? string.Empty;
            if (int.TryParse(configuration[$"{PlatformFirebaseOptions.SectionName}:DispatchIntervalSeconds"],
                    out var interval)) options.DispatchIntervalSeconds = interval;
            if (int.TryParse(configuration[$"{PlatformFirebaseOptions.SectionName}:BatchSize"],
                    out var batchSize)) options.BatchSize = batchSize;
        });
        services.AddSingleton<IPlatformPushSender, FirebasePlatformPushSender>();
        services.AddScoped<IPlatformNotificationService, PlatformNotificationService>();
        services.AddHostedService<PlatformNotificationDispatchWorker>();
        services.AddScoped<IRegistryReadStore, RegistryReadStore>();
        services.AddScoped<IPlatformRegistryAuditWriter, PlatformRegistryAuditWriter>();
        services.AddScoped<CreateTenantCommandHandler>();
        services.AddScoped<RegisterSchoolCommandHandler>();
        services.AddScoped<BeginSchoolProvisioningCommandHandler>();
        services.AddScoped<RegistryReadService>();
        services.AddScoped<TransitionSchoolCommandHandler>();
        services.AddScoped<IPlatformPaymentRequestRepository, PlatformPaymentRequestRepository>();
        services.AddScoped<IPlatformBillingAuditWriter, PlatformBillingAuditWriter>();
        services.AddScoped<IPlatformBillingUnitOfWork, PlatformBillingUnitOfWork>();
        services.AddScoped<IPlatformPaymentReadStore, PlatformPaymentReadStore>();
        services.AddScoped<IUnitTypeRepository, UnitTypeRepository>();
        services.AddScoped<IUnitPurchaseRepository, UnitPurchaseRepository>();
        services.AddScoped<IUnitCommerceAuditWriter, UnitCommerceAuditWriter>();
        services.AddScoped<SubmitSchoolPlatformPaymentCommandHandler>();
        services.AddScoped<ReviewSchoolPlatformPaymentCommandHandler>();
        services.AddScoped<CreateUnitTypeCommandHandler>();
        services.AddScoped<UpdateUnitTypeCommandHandler>();
        services.AddScoped<DeactivateUnitTypeCommandHandler>();
        services.AddScoped<ListUnitTypesQueryHandler>();
        services.AddScoped<SubmitUnitPurchaseCommandHandler>();
        services.AddScoped<IPlatformStaffDirectory, PlatformStaffDirectory>();
        services.AddScoped<IPlatformStaffRoleManager, PlatformStaffRoleManager>();
        services.AddScoped<IPlatformStaffManagement, PlatformStaffManagement>();
        services.AddScoped<IPlatformRoleCatalog, PlatformRoleCatalog>();
        services.AddScoped<IPlatformRoleManager, PlatformRoleManager>();
        services.TryAddSingleton<IClock>(SystemClock.Instance);

        services
            .AddHealthChecks()
            .AddCheck(
                "platform-database",
                provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase)
                    ? new PostgreSqlConnectionHealthCheck(platformConnection)
                    : new SqlConnectionHealthCheck(platformConnection),
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready"],
                timeout: TimeSpan.FromSeconds(5))
            .AddCheck(
                "identity-database",
                provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase)
                    ? new PostgreSqlConnectionHealthCheck(identityConnection)
                    : new SqlConnectionHealthCheck(identityConnection),
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready"],
                timeout: TimeSpan.FromSeconds(5));

        return services;
    }

    private static string GetRequiredConnectionString(
        IConfiguration configuration,
        string connectionName)
    {
        var connectionString = configuration.GetConnectionString(connectionName);

        return string.IsNullOrWhiteSpace(connectionString)
            ? throw new InvalidOperationException(
                $"The connection string 'ConnectionStrings:{connectionName}' is required.")
            : connectionString;
    }

    private static void ConfigureSqlServer(
        DbContextOptionsBuilder options,
        string connectionString,
        string migrationsHistorySchema)
    {
        options.UseSqlServer(
            connectionString,
            sql =>
            {
                sql.MigrationsAssembly(typeof(ServiceCollectionExtensions).Assembly.FullName);
                sql.MigrationsHistoryTable("__EFMigrationsHistory", migrationsHistorySchema);
                sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });
    }

    private static void ConfigurePostgreSql(
        DbContextOptionsBuilder options, string connectionString, string historySchema)
    {
        options.UseNpgsql(connectionString, pg =>
        {
            pg.MigrationsAssembly(typeof(ServiceCollectionExtensions).Assembly.FullName);
            pg.MigrationsHistoryTable("__EFMigrationsHistory", historySchema);
            pg.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        });
    }

}
