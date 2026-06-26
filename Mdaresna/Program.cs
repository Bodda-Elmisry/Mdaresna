using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.Helpers;
using Mdaresna.Infrastructure.Configrations;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Hubs;
using Mdaresna.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Mdaresna
{
    public class Program
    {
        public static void Main(string[] args)
        {

            //Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));

            var builder = WebApplication.CreateBuilder(args);

            //var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            // Add services to the container.
            //Log.Logger = new LoggerConfiguration()
            ////.ReadFrom.Configuration(builder.Configuration)
            ////.Enrich.FromLogContext()
            ////.WriteTo.Console()
            ////.WriteTo.File("C:/Logs/Mdaresnalog-.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
            //.ReadFrom.Configuration(config)
            //.CreateLogger();
            //builder.Host.UseSerilog();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var jwtSection = builder.Configuration.GetSection("Jwt");
            var keyBytes = Encoding.UTF8.GetBytes(jwtSection["Key"] ?? "MdaresnaAPISecretKeyMustBeVeryLong32Chars!");
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddCors(oprions =>
            {
                oprions.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("http://localhost:5173");

                });
                oprions.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin());
            });

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(
                                builder.Configuration.GetConnectionString("DefaultConnection")
                            //sqlServerOptions =>
                            //{
                            //    sqlServerOptions.EnableRetryOnFailure();
                            //}
                            ).EnableSensitiveDataLogging().LogTo(Console.WriteLine, LogLevel.Information),
                            ServiceLifetime.Scoped
                    );


            builder.Services.AddDbContext<AppMainDbContext>(options =>
                    options.UseSqlServer(
                                builder.Configuration.GetConnectionString("MainConnection")
                            //sqlServerOptions =>
                            //{
                            //    sqlServerOptions.EnableRetryOnFailure();
                            //}
                            ).EnableSensitiveDataLogging().LogTo(Console.WriteLine, LogLevel.Information),
                            ServiceLifetime.Scoped
                    );
            builder.Services.Configure<AppSettingDTO>(builder.Configuration.GetSection("AppSettings"));
            builder.Services.AddMemoryCache();

            DependencyInjectionConfig.ConfigerRepositories(builder.Services);
            DependencyInjectionConfig.ConfigerHubs(builder.Services);
            DependencyInjectionConfig.ConfigerFactories(builder.Services);
            DependencyInjectionConfig.ConfigerServices(builder.Services);
            DependencyInjectionConfig.ConfigerMainDB(builder.Services);

            //builder.WebHost.ConfigureKestrel(options =>
            //{
            //    options.ListenAnyIP(80);

            //    options.ListenAnyIP(443, listenOptions =>
            //    {
            //        listenOptions.UseHttps(
            //            @"C:\certs\mdaresna_api_fullchain.pfx",
            //            "Aly@1091010");
            //    });
            //});

            var app = builder.Build();

            app.Use(async (context, next) =>
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var requestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var method = context.Request.Method;
                var path = context.Request.Path;

                try
                {
                    await next();
                }
                finally
                {
                    stopwatch.Stop();
                    var duration = stopwatch.ElapsedMilliseconds;
                    var statusCode = context.Response.StatusCode;
                    var logLine = $"[{requestTime}] {method} {path} - Status: {statusCode} - Time: {duration} ms{Environment.NewLine}";

                    try
                    {
                        var logDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                        if (!Directory.Exists(logDir))
                        {
                            Directory.CreateDirectory(logDir);
                        }
                        var logFileName = $"diagnostics_{DateTime.Now:yyyyMMdd}.log";
                        var logFilePath = Path.Combine(logDir, logFileName);
                        await System.IO.File.AppendAllTextAsync(logFilePath, logLine);
                    }
                    catch
                    {
                        // Fail-safe to prevent request failure if writing to log file fails
                    }
                }
            });

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}

            //app.UseSerilogRequestLogging();

            app.UseCors(builder => builder.AllowAnyOrigin()
                                          .AllowAnyMethod()
                                          .AllowAnyHeader());

            app.UseMiddleware<SetAppUrlMiddleware>();
            app.UseMiddleware<VpnBlockingMiddleware>();
            app.UseMiddleware<AppCheckMiddleware>();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                        Path.Combine(Directory.GetCurrentDirectory(), "Images")),
                RequestPath = "/Images"
            });
            app.UseRouting();

            app.UseSwagger();
            app.UseSwaggerUI();


            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapHub<NotificationHub>("/notificationHub");

            app.MapControllers();

            app.Run();

            //try
            //{
            //    Log.Information("Application Will Run");

            //    app.Run();
            //}
            //catch (Exception ex)
            //{
            //    Log.Fatal(ex, "Application terminated unexpectedly.");
            //}
            //finally
            //{
            //    Log.Information("Application Finally");
            //    Log.CloseAndFlush();
            //}
        }

    }
}