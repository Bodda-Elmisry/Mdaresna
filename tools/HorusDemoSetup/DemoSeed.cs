using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Facilities;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.Organization;
using Mdaresna.Schools.Domain.School;
using Mdaresna.Schools.Domain.Students;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

internal static partial class DemoSeed
{
    internal const string Prefix = "HDEMO";
    internal static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;
    internal static readonly DateOnly End = new(2027, 6, 24);
    internal static Guid Id(string key) => new(SHA256.HashData(Encoding.UTF8.GetBytes("HorusDemo-v1/SCH-1A769BC8A279/" + key)).AsSpan(0, 16));

    internal static T Add<T>(SchoolsDbContext db, string key, T entity) where T : class
    {
        typeof(T).GetProperty("Id")?.SetValue(entity, Id(key));
        typeof(T).GetProperty("CreatedAtUtc")?.SetValue(entity, Now);
        typeof(T).GetProperty("UpdatedAtUtc")?.SetValue(entity, Now);
        db.Add(entity); return entity;
    }

    public static async Task ApplyAsync(SchoolsDbContext db, IGlobalStudentGateway global, ISchoolUserIdentityGateway identities, Guid owner,
        IConfiguration config, IConfiguration platform)
    {
        if (await db.EducationPrograms.AnyAsync(x => x.Code.StartsWith(Prefix)))
        {
            await VerifyAsync(db, false);
            await CompleteConfigurationAsync(db, owner);
            await VerifyAsync(db);
            return;
        }
        if (await db.Students.AnyAsync() || await db.ClassSections.AnyAsync() || await db.EducationPrograms.AnyAsync())
            throw new InvalidOperationException("Expected an empty Horus academic setup. Existing data was not changed.");
        var license = await db.Subscriptions.SingleOrDefaultAsync(x => x.CapacityMetric == "Student");
        if (license is null || !license.IsEnforced || license.IsSuspended || license.UsedCapacity != 0 ||
            license.BaseCapacity + license.AddOnCapacity < 110 || license.ValidFromUtc > Now || license.ValidUntilUtc <= Now)
            throw new InvalidOperationException("An active, initialized subscription with 110 available student seats is required.");
        await BackupAsync(db.Database.GetConnectionString()!, "horus_before_demo");
        await BackupAsync(platform.GetConnectionString("PlatformConnection")!, "platform_before_horus_demo");
        await BackupAsync(platform.GetConnectionString("IdentityConnection")!, "identity_before_horus_demo");
        var staffPassword = StaffPassword(config);
        // All school data and seat allocations commit together. Global identities are idempotent by demo certificate reference.
        await using var tx = await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(710191103)");
        await BuildAsync(db, global, identities, owner, staffPassword);
        await CompleteConfigurationAsync(db, owner);
        await db.SaveChangesAsync();
        await VerifyAsync(db, false);
        await tx.CommitAsync();
        await VerifyAsync(db);
        Console.WriteLine("Horus demo setup committed. No SMS, email, activation or payment was sent.");
    }

    private static async Task CompleteConfigurationAsync(SchoolsDbContext db, Guid owner)
    {
        var school = await db.SchoolInformation.SingleAsync();
        school.ConfigureStudentAttendance("Africa/Cairo", StudentAttendanceMode.Daily, DateTimeOffset.UtcNow);
        var codes = new[] { "school.academics.view", "school.operations.view", "school.facilities.view", "school.departments.view", "school.lesson_notes.view", "school.lesson_notes.manage", "school.lesson_evaluations.view", "school.lesson_evaluations.manage" };
        var permissions = await db.LocalPermissions.Where(x => x.IsActive && codes.Contains(x.Code)).ToArrayAsync();
        foreach (var role in await db.LocalRoles.Where(x => x.Code == "horus-demo-teacher" || x.Code == "horus-demo-coteacher" || x.Code == "horus-demo-supervisor").ToArrayAsync())
        {
            var current = await db.Set<LocalRolePermission>().Where(x => x.RoleId == role.Id).Select(x => x.PermissionId).ToArrayAsync();
            foreach (var p in permissions.Where(p => !current.Contains(p.Id)))
                db.Add(new LocalRolePermission { RoleId = role.Id, PermissionId = p.Id, GrantedAtUtc = DateTimeOffset.UtcNow, GrantedByUserId = owner });
        }
        await db.SaveChangesAsync();
        Console.WriteLine("Demo classroom permissions and Africa/Cairo daily attendance configured.");
    }

    private static string StaffPassword(IConfiguration config)
    {
        var password = config["SchoolDemo:HorusStaffPassword"];
        if (!string.IsNullOrWhiteSpace(password)) return password;
        password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24)) + "!aA1";
        var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "UserSecrets", "Mdaresna.Schools.Api", "secrets.json");
        var json = File.Exists(file) ? JsonNode.Parse(File.ReadAllText(file))!.AsObject() : new JsonObject();
        json["SchoolDemo:HorusStaffPassword"] = password;
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.WriteAllText(file, json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Demo staff password saved to Schools API User Secrets (value not printed).");
        return password;
    }

    private static async Task BackupAsync(string connection, string label)
    {
        var target = new Npgsql.NpgsqlConnectionStringBuilder(connection);
        if (target.Host is not ("localhost" or "127.0.0.1") || target.Port != 5432) throw new InvalidOperationException("Refusing nonlocal backup target.");
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mdaresna", "Backups");
        Directory.CreateDirectory(folder);
        var file = Path.Combine(folder, label + "_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".dump");
        async Task<string> Run(string executable, string[] arguments, bool credentials)
        {
            var start = new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PostgreSQL", "17", "bin", executable))
                { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
            foreach (var arg in arguments) start.ArgumentList.Add(arg);
            if (credentials) { start.Environment["PGPASSWORD"] = target.Password; start.Environment["PGCONNECT_TIMEOUT"] = "8"; }
            using var process = Process.Start(start)!; var output = process.StandardOutput.ReadToEndAsync(); var error = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync(); await error;
            if (process.ExitCode != 0) throw new InvalidOperationException("Database backup/verification failed. No setup was started.");
            return await output;
        }
        await Run("pg_dump.exe", ["--host", target.Host!, "--port", target.Port.ToString(), "--username", target.Username!, "--dbname", target.Database!, "--format=custom", "--file", file, "--no-password"], true);
        if (new FileInfo(file).Length < 1 || !(await Run("pg_restore.exe", ["--list", file], false)).Contains("TABLE")) throw new InvalidOperationException("Invalid backup.");
        Console.WriteLine("Backup verified: " + file);
    }
}
