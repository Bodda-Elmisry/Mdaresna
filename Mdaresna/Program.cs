using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.Helpers;
using Mdaresna.Infrastructure.Configrations;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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
                ),
                ServiceLifetime.Singleton
        );

builder.Services.Configure<AppSettingDTO>(builder.Configuration.GetSection("AppSettings"));

DependencyInjectionConfig.ConfigerRepositories(builder.Services);
DependencyInjectionConfig.ConfigerServices(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

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

app.MapControllers();

app.Run();
