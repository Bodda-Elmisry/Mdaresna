using Mdaresna.Platform.Bootstrap;
using Mdaresna.Platform.Infrastructure.Messaging;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text;

try
{
    var provider = Environment.GetEnvironmentVariable("PlatformDatabase__Provider") ?? "SqlServer";
    var usePostgreSql = provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase);
    if (!usePostgreSql && !provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        throw new BootstrapRejectedException("PlatformDatabase__Provider must be SqlServer or PostgreSql.");

    if (args.Length > 0 && args[0] == "--seed-sms-provider")
    {
        var seedOptions = SmsProviderSeedOptions.Parse(args);
        var seedConnection = RequireConnection("ConnectionStrings__PlatformConnection");
        EnsureExplicitPlatformTarget(seedConnection, usePostgreSql);
        await using PlatformDbContext seedDb = usePostgreSql
            ? new PostgreSqlPlatformDbContext(
                new DbContextOptionsBuilder<PostgreSqlPlatformDbContext>()
                    .UseNpgsql(seedConnection, pg =>
                        pg.MigrationsHistoryTable("__EFMigrationsHistory", "platform")).Options)
            : new PlatformDbContext(
                new DbContextOptionsBuilder<PlatformDbContext>()
                    .UseSqlServer(seedConnection, sql =>
                        sql.MigrationsHistoryTable("__EFMigrationsHistory", "platform")).Options);

        if (seedOptions.DryRun)
        {
            // A dry run does not need or prompt for provider secrets.
            Console.WriteLine(await new SmsProviderSeedBootstrapper(seedDb).InspectAsync());
            return 0;
        }

        var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
        var protector = new PlatformSmsSecretProtector(configuration);
        var password = ReadSeedPassword();
        var seedResult = await new SmsProviderSeedBootstrapper(seedDb, protector)
            .RunAsync(password);
        Console.WriteLine(seedResult.AlreadyCompleted
            ? $"Initial Platform SMS provider already exists ({seedResult.ProviderId:D}); no changes made."
            : $"Initial Platform SMS provider provisioned ({seedResult.ProviderId:D}).");
        return 0;
    }

    var importLocalUsers = args.Length > 0 && args[0] == "--import-platform-local-users";
    var importExecute = importLocalUsers && args.Length == 2 && args[1] == "--execute";
    if (importLocalUsers && args.Length > 1 && !importExecute)
        throw new BootstrapUsageException("Invalid local-account import argument.");
    var moveCredential = args.Length > 0 && args[0] == "--move-platform-credential";
    var moveExecute = moveCredential && args.Length == 4 && args[3] == "--execute";
    if (moveCredential && (args.Length is not (3 or 4) ||
        args[1] != "--person-id" ||
        !Guid.TryParse(args[2], out var parsedPersonId) ||
        parsedPersonId == Guid.Empty ||
        args.Length == 4 && !moveExecute))
        throw new BootstrapUsageException("Invalid credential-move argument.");
    var options = importLocalUsers || moveCredential ? null : BootstrapOptions.Parse(args);
    var platformConnection = RequireConnection("ConnectionStrings__PlatformConnection");
    var identityConnection = RequireConnection("ConnectionStrings__IdentityConnection");
    EnsureSeparateDatabaseTargets(platformConnection, identityConnection, usePostgreSql);

    await using PlatformDbContext platformDb = usePostgreSql
        ? new PostgreSqlPlatformDbContext(
            new DbContextOptionsBuilder<PostgreSqlPlatformDbContext>()
                .UseNpgsql(platformConnection, pg =>
                    pg.MigrationsHistoryTable("__EFMigrationsHistory", "platform")).Options)
        : new PlatformDbContext(
            new DbContextOptionsBuilder<PlatformDbContext>()
                .UseSqlServer(platformConnection, sql =>
                    sql.MigrationsHistoryTable("__EFMigrationsHistory", "platform")).Options);
    await using IdentityDbContext identityDb = usePostgreSql
        ? new PostgreSqlIdentityDbContext(
            new DbContextOptionsBuilder<PostgreSqlIdentityDbContext>()
                .UseNpgsql(identityConnection, pg =>
                    pg.MigrationsHistoryTable("__EFMigrationsHistory", "identity")).Options)
        : new IdentityDbContext(
            new DbContextOptionsBuilder<IdentityDbContext>()
                .UseSqlServer(identityConnection, sql =>
                    sql.MigrationsHistoryTable("__EFMigrationsHistory", "identity")).Options);
    if (importLocalUsers)
    {
        var import = await new PlatformLocalAccountImporter(identityDb, platformDb)
            .RunAsync(dryRun: !importExecute);
        Console.WriteLine($"Platform local accounts: total={import.Total}, " +
            $"imported={import.Imported}, " +
            $"unchanged={import.Skipped}, dryRun={import.DryRun}.");
        return 0;
    }
    if (moveCredential)
    {
        var personId = Guid.Parse(args[2]);
        var move = await new PlatformCredentialMover(identityDb, platformDb)
            .RunAsync(personId, moveExecute);
        Console.WriteLine(move.AlreadyMoved
            ? "Platform credential was already local; no change made."
            : move.Executed
                ? "Platform credential moved; legacy Identity credential removed."
                : "Credential move is eligible; dry-run only. Use --execute to move it.");
        return 0;
    }

    var bootstrapper = new FirstOwnerBootstrapper(identityDb, platformDb);

    if (options!.DryRun)
    {
        Console.WriteLine(await bootstrapper.InspectAsync(options));
        return 0;
    }

    var result = await bootstrapper.RunAsync(options);

    Console.WriteLine(result.AlreadyCompleted
        ? $"First-owner provisioning already completed for account {result.AccountId:D}."
        : $"First App Manager provisioned pending phone activation for account {result.AccountId:D}.");
    return 0;
}
catch (BootstrapUsageException exception)
{
    if (!string.IsNullOrWhiteSpace(exception.Message))
    {
        Console.Error.WriteLine(exception.Message);
    }

    Console.Error.WriteLine(
        "Usage: dotnet run --project src/Platform/Mdaresna.Platform.Bootstrap -- " +
        "--display-name <name> --operation-id <stable-GUID> [--dry-run]");
    Console.Error.WriteLine(
        "   or: dotnet run --project src/Platform/Mdaresna.Platform.Bootstrap -- " +
        "--seed-sms-provider [--dry-run]");
    Console.Error.WriteLine(
        "   or: dotnet run --project src/Platform/Mdaresna.Platform.Bootstrap -- " +
        "--import-platform-local-users [--execute] (dry-run by default)");
    Console.Error.WriteLine(
        "   or: dotnet run --project src/Platform/Mdaresna.Platform.Bootstrap -- " +
        "--move-platform-credential --person-id <GUID> [--execute] (dry-run by default)");
    Console.Error.WriteLine(
        "Provide ConnectionStrings__PlatformConnection for SMS provider seeding, " +
        "or both Platform and Identity connections for first-owner provisioning. " +
        "The fixed first-owner phone is configured in the bootstrap tool. " +
        "No phone verification or password setup occurs in this command.");
    Console.Error.WriteLine(
        "SMS provider seeding also requires PlatformSms__EncryptionKey and " +
        "PlatformSms__SeedPassword, or an interactive masked password entry.");
    return string.IsNullOrWhiteSpace(exception.Message) ? 0 : 2;
}
catch (BootstrapRejectedException exception)
{
    Console.Error.WriteLine(exception.Message);
    return 3;
}
catch (Exception)
{
    // DB/provider exception messages may contain connection details or other
    // sensitive values. Preserve no secrets in ordinary CLI output.
    Console.Error.WriteLine(
        "Bootstrap failed closed. No connection details were printed. " +
        "Inspect database state before retrying with the SAME operation ID.");
    return 4;
}

static string RequireConnection(string variable)
{
    var value = Environment.GetEnvironmentVariable(variable);
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new BootstrapRejectedException($"Environment variable {variable} is required.");
    }

    return value;
}

static void EnsureSeparateDatabaseTargets(
    string platformConnection, string identityConnection, bool usePostgreSql)
{
    if (usePostgreSql)
    {
        NpgsqlConnectionStringBuilder platformPg;
        NpgsqlConnectionStringBuilder identityPg;
        try
        {
            platformPg = new NpgsqlConnectionStringBuilder(platformConnection);
            identityPg = new NpgsqlConnectionStringBuilder(identityConnection);
        }
        catch (ArgumentException)
        {
            throw new BootstrapRejectedException("Both connection strings must be valid PostgreSQL targets.");
        }
        if (string.IsNullOrWhiteSpace(platformPg.Host) ||
            string.IsNullOrWhiteSpace(identityPg.Host) ||
            string.IsNullOrWhiteSpace(platformPg.Database) ||
            string.IsNullOrWhiteSpace(identityPg.Database) ||
            platformPg.Host.Equals(identityPg.Host, StringComparison.OrdinalIgnoreCase) &&
            platformPg.Port == identityPg.Port &&
            platformPg.Database.Equals(identityPg.Database, StringComparison.OrdinalIgnoreCase))
        {
            throw new BootstrapRejectedException(
                "Platform and Identity require explicit, different PostgreSQL databases.");
        }
        return;
    }

    SqlConnectionStringBuilder platform;
    SqlConnectionStringBuilder identity;
    try
    {
        platform = new SqlConnectionStringBuilder(platformConnection);
        identity = new SqlConnectionStringBuilder(identityConnection);
    }
    catch (ArgumentException)
    {
        throw new BootstrapRejectedException("Both connection strings must be valid SQL Server targets.");
    }

    if (string.IsNullOrWhiteSpace(platform.DataSource) ||
        string.IsNullOrWhiteSpace(identity.DataSource) ||
        string.IsNullOrWhiteSpace(platform.InitialCatalog) ||
        string.IsNullOrWhiteSpace(identity.InitialCatalog))
    {
        throw new BootstrapRejectedException(
            "Both connection strings need an explicit server and database name.");
    }

    if (string.Equals(platform.DataSource.Trim(), identity.DataSource.Trim(),
            StringComparison.OrdinalIgnoreCase) &&
        string.Equals(platform.InitialCatalog.Trim(), identity.InitialCatalog.Trim(),
            StringComparison.OrdinalIgnoreCase))
    {
        throw new BootstrapRejectedException(
            "Platform and Identity connections must target different databases.");
    }
}

static void EnsureExplicitPlatformTarget(string platformConnection, bool usePostgreSql)
{
    if (usePostgreSql)
    {
        try
        {
            var pgTarget = new NpgsqlConnectionStringBuilder(platformConnection);
            if (!string.IsNullOrWhiteSpace(pgTarget.Host) &&
                !string.IsNullOrWhiteSpace(pgTarget.Database)) return;
        }
        catch (ArgumentException) { }
        throw new BootstrapRejectedException(
            "The Platform connection requires an explicit PostgreSQL host and database.");
    }

    SqlConnectionStringBuilder target;
    try
    {
        target = new SqlConnectionStringBuilder(platformConnection);
    }
    catch (ArgumentException)
    {
        throw new BootstrapRejectedException(
            "The Platform connection string must be a valid SQL Server target.");
    }

    if (string.IsNullOrWhiteSpace(target.DataSource) ||
        string.IsNullOrWhiteSpace(target.InitialCatalog))
    {
        throw new BootstrapRejectedException(
            "The Platform connection requires an explicit server and database name.");
    }
}

static string ReadSeedPassword()
{
    var supplied = Environment.GetEnvironmentVariable("PlatformSms__SeedPassword");
    if (!string.IsNullOrWhiteSpace(supplied))
    {
        return supplied;
    }

    if (Console.IsInputRedirected)
    {
        throw new BootstrapRejectedException(
            "PlatformSms__SeedPassword is required when interactive input is unavailable.");
    }

    Console.Error.Write("SMS provider password (input hidden): ");
    var buffer = new StringBuilder();
    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter)
        {
            Console.Error.WriteLine();
            break;
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (buffer.Length > 0)
            {
                buffer.Length--;
            }
            continue;
        }

        if (!char.IsControl(key.KeyChar) && buffer.Length < 301)
        {
            buffer.Append(key.KeyChar);
        }
    }

    return buffer.ToString();
}
