using Mdaresna.Doamin.MainDB.Models;
using Mdaresna.Doamin.MainDB.ModelsConfigurations;
using Mdaresna.Doamin.MainDB.ModelsSeeding;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Infrastructure.Data;

public class AppMainDbContext : DbContext
{
    public AppMainDbContext(DbContextOptions<AppMainDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new MdaresnaSchoolConfig());
        modelBuilder.ApplyConfiguration(new MdaresnaServiceConfig());
        modelBuilder.ApplyConfiguration(new MdaresnaSchoolServiceConfig());

        modelBuilder.ApplyConfiguration(new MdaresnaServiceSeed());
    }

    public DbSet<MdaresnaSchool> Schools { get; set; }
    public DbSet<MdaresnaService> Services { get; set; }
    public DbSet<MdaresnaSchoolService> SchoolServices { get; set; }

}
