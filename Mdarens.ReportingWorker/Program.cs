using Hangfire;
using Hangfire.SqlServer;
using Mdarens.ReportingWorker;
using Mdarens.ReportingWorker.Factories;
using Mdarens.ReportingWorker.Services;
using Mdaresna.Infrastructure.Configrations;
using Mdaresna.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "MdarensReportingWorker";
});

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
var mainConnection = builder.Configuration.GetConnectionString("MainConnection")
    ?? builder.Configuration.GetConnectionString("AdminConnection")
    ?? throw new InvalidOperationException("Connection string 'MainConnection' is missing.");
var hangfireConnection = builder.Configuration.GetConnectionString("HangfireConnection")
    ?? throw new InvalidOperationException("Connection string 'HangfireConnection' is missing.");

await EnsureSqlServerDatabaseExistsAsync(hangfireConnection);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(defaultConnection));

builder.Services.AddDbContext<AppMainDbContext>(options =>
    options.UseSqlServer(mainConnection));

DependencyInjectionConfig.ConfigerRepositories(builder.Services);
DependencyInjectionConfig.ConfigerHubs(builder.Services);
DependencyInjectionConfig.ConfigerFactories(builder.Services);
DependencyInjectionConfig.ConfigerServices(builder.Services);
DependencyInjectionConfig.ConfigerMainDB(builder.Services);
builder.Services.AddScoped<ISchoolDbContextFactory, SchoolDbContextFactory>();
builder.Services.AddScoped<IReportProcessingService, ReportProcessingService>();
builder.Services.AddSingleton<IWorkerJobSettingsService, WorkerJobSettingsService>();

builder.Services.AddScoped<IReportingJob, ReportingJob>();

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(hangfireConnection, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

builder.Services.AddHangfireServer(options =>
{
    options.Queues = new[] { "reports", "default" };
});

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate<IReportingJob>(
        "process-pending-report-queues",
        job => job.ProcessPendingReportsAsync(CancellationToken.None),
        Cron.Minutely());
}

host.Run();

static async Task EnsureSqlServerDatabaseExistsAsync(
    string connectionString,
    CancellationToken cancellationToken = default)
{
    var connectionBuilder = new SqlConnectionStringBuilder(connectionString);
    var databaseName = connectionBuilder.InitialCatalog;

    if (string.IsNullOrWhiteSpace(databaseName))
    {
        throw new InvalidOperationException("HangfireConnection must include a database name.");
    }

    connectionBuilder.InitialCatalog = "master";

    await using var connection = new SqlConnection(connectionBuilder.ConnectionString);
    await connection.OpenAsync(cancellationToken);

    await using var databaseExistsCommand = connection.CreateCommand();
    databaseExistsCommand.CommandText = "SELECT COUNT(1) FROM sys.databases WHERE [name] = @databaseName;";
    databaseExistsCommand.Parameters.AddWithValue("@databaseName", databaseName);

    var databaseExists = Convert.ToInt32(
        await databaseExistsCommand.ExecuteScalarAsync(cancellationToken)) > 0;

    if (databaseExists)
    {
        return;
    }

    await using var createDatabaseCommand = connection.CreateCommand();
    createDatabaseCommand.CommandText = @"
DECLARE @sql nvarchar(max) = N'CREATE DATABASE ' + QUOTENAME(@databaseName);
EXEC (@sql);";
    createDatabaseCommand.Parameters.AddWithValue("@databaseName", databaseName);

    await createDatabaseCommand.ExecuteNonQueryAsync(cancellationToken);
}
