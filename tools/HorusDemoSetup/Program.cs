using System.Net.Http.Json;
using System.Text.Json;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.DependencyInjection;
using Mdaresna.Schools.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

const string schoolCode = "SCH-1A769BC8A279";
var root = Directory.GetCurrentDirectory();
if (!Directory.Exists(Path.Combine(root, "src", "Schools"))) throw new InvalidOperationException("Run from the Mdaresna repository root.");
IConfiguration Config(string area, string name) => new ConfigurationBuilder().SetBasePath(Path.Combine(root, "src", area, name))
    .AddJsonFile("appsettings.json").AddJsonFile("appsettings.Development.json", true).AddUserSecrets(name, true).AddEnvironmentVariables().Build();
var config = Config("Schools", "Mdaresna.Schools.Api");
var services = new ServiceCollection(); services.AddSingleton(config); services.AddLogging(); services.AddSchoolsInfrastructure(config);
await using var provider = services.BuildServiceProvider(); await using var scope = provider.CreateAsyncScope();
var factory = scope.ServiceProvider.GetRequiredService<ISchoolDbContextFactory>();
await using var db = await factory.CreateAsync(schoolCode) ?? throw new InvalidOperationException("Horus is unavailable.");
if (!db.Database.IsNpgsql()) throw new InvalidOperationException("This local demo setup supports PostgreSQL only.");
var connection = new Npgsql.NpgsqlConnectionStringBuilder(db.Database.GetConnectionString());
if (connection.Host is not ("localhost" or "127.0.0.1") || connection.Port != 5432) throw new InvalidOperationException("Expected the local development database.");
var school = await db.SchoolInformation.SingleAsync();
if (school.Code != schoolCode || !school.DisplayName.Contains("حورس")) throw new InvalidOperationException("Unexpected school.");
Console.WriteLine($"Target: {school.DisplayName} ({school.Code})");
Console.WriteLine($"Programs={await db.EducationPrograms.CountAsync()}, classes={await db.ClassSections.CountAsync()}, students={await db.Students.CountAsync()}, users={await db.LocalUsers.CountAsync()}");
foreach (var s in await db.Subscriptions.AsNoTracking().ToArrayAsync()) Console.WriteLine($"Subscription {s.CapacityMetric}: {s.UsedCapacity}/{s.BaseCapacity + s.AddOnCapacity}; enforced={s.IsEnforced}");
Console.WriteLine("Roles: " + string.Join(", ", await db.LocalRoles.Select(x => x.Code).ToArrayAsync()));
var pending = (await db.Database.GetPendingMigrationsAsync()).ToArray();
Console.WriteLine("Pending migrations=" + pending.Length);
if (args.Contains("--inspect")) return;
if (args.Contains("--self-test")) { await DemoSeed.SelfTestAsync(db); return; }
if (args.Contains("--verify")) { await DemoSeed.VerifyAsync(db); return; }
if (!args.Contains("--apply")) { Console.WriteLine("Use --inspect, --verify, or --apply. No setup data written."); return; }
if (pending.Length != 0) throw new InvalidOperationException("Apply the reviewed school migrations before setup.");
var password = Environment.GetEnvironmentVariable("MDARESNA_DEMO_OWNER_PASSWORD") ?? throw new InvalidOperationException("MDARESNA_DEMO_OWNER_PASSWORD is required.");
using var http = new HttpClient { BaseAddress = new Uri("http://localhost:5251"), Timeout = TimeSpan.FromSeconds(15) };
using var login = await http.PostAsJsonAsync("/api/schools/v1/auth/login", new { login = "owner@" + schoolCode, password });
if (!login.IsSuccessStatusCode) throw new InvalidOperationException($"Owner login failed: HTTP{(int)login.StatusCode}.");
using var body = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
var session = body.RootElement.GetProperty("data");
if (session.GetProperty("schoolCode").GetString() != schoolCode || session.GetProperty("schoolId").GetGuid() != school.PlatformSchoolReferenceId ||
    !session.GetProperty("roles").EnumerateArray().Any(x => x.GetString() == SchoolIdentitySeed.SchoolAdminRoleCode))
    throw new InvalidOperationException("An authenticated Horus owner is required.");
Console.WriteLine("Owner authenticated; credentials were not printed or saved.");
await DemoSeed.ApplyAsync(db, scope.ServiceProvider.GetRequiredService<IGlobalStudentGateway>(), scope.ServiceProvider.GetRequiredService<ISchoolUserIdentityGateway>(),
    session.GetProperty("userId").GetGuid(), config, Config("Platform", "Mdaresna.Platform.Api"));
