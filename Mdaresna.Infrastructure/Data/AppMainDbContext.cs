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

        modelBuilder.RegisterUtcTimeZoneConverters();
    }

    public DbSet<MdaresnaSchool> Schools { get; set; }
    public DbSet<MdaresnaService> Services { get; set; }
    public DbSet<MdaresnaSchoolService> SchoolServices { get; set; }

    public override int SaveChanges()
    {
        this.ConvertAllDatesToUtc();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        this.ConvertAllDatesToUtc();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        this.ConvertAllDatesToUtc();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        this.ConvertAllDatesToUtc();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}

