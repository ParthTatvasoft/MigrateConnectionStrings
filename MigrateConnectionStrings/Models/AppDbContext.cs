using Microsoft.EntityFrameworkCore;
using MigrateConnectionStrings.Models;

public class AppDbContext : DbContext
{
    public DbSet<AppConfigurations> AppConfigurations { get; set; }
    public DbSet<Agencies> Agencies { get; set; }
    public DbSet<AgencyApps> AgencyApps { get; set; }
    public DbSet<Apps> Apps { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agencies>(entity =>
        {
            entity.HasKey(e => e.AgencyId)
                .HasName("PK__Agencies");
        });

        modelBuilder.Entity<AgencyApps>(entity =>
        {
            entity.HasKey(e => new { e.AgencyId, e.AppId, e.Display })
                .HasName("PK_AgencyApps");
        });

        modelBuilder.Entity<AppConfigurations>(entity =>
        {
            entity.ToTable("AppConfigurations");

            entity.HasKey(e => e.AppConfigurationId)
                .HasName("AppConfigurations");
        });

        modelBuilder.Entity<Apps>(entity =>
        {
            entity.HasKey(e => e.AppId)
                .HasName("PK__Apps");
        });
    }
}
