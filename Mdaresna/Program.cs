using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.Helpers;
using Mdaresna.Infrastructure.Configrations;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Hubs;
using Mdaresna.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Serilog;

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

            builder.Services.Configure<AppSettingDTO>(builder.Configuration.GetSection("AppSettings"));

            DependencyInjectionConfig.ConfigerRepositories(builder.Services);
            DependencyInjectionConfig.ConfigerHubs(builder.Services);
            DependencyInjectionConfig.ConfigerFactories(builder.Services);
            DependencyInjectionConfig.ConfigerServices(builder.Services);

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

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}

            //app.UseSerilogRequestLogging();

            app.UseMiddleware<SetAppUrlMiddleware>();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                        Path.Combine(Directory.GetCurrentDirectory(), "Images")),
                RequestPath = "/Images"
            });
            app.UseRouting();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseCors(builder => builder.AllowAnyOrigin()
                                          .AllowAnyMethod()
                                          .AllowAnyHeader());


            app.UseHttpsRedirection();

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