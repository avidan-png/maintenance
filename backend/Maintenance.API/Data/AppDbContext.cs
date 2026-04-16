using Microsoft.EntityFrameworkCore;
using Maintenance.API.Models;

namespace Maintenance.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Setting> Settings => Set<Setting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Setting>()
            .HasIndex(s => s.Key)
            .IsUnique();

        modelBuilder.Entity<Contract>()
            .Property(c => c.MontantHtAnnuel)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Setting>().HasData(
            new Setting { Id = 1, Key = "preavis_default_mois", Value = "3" },
            new Setting { Id = 2, Key = "alert_6mois",          Value = "true" },
            new Setting { Id = 3, Key = "alert_3mois",          Value = "true" },
            new Setting { Id = 4, Key = "alert_1mois",          Value = "true" },
            new Setting { Id = 5, Key = "alert_depasse",        Value = "true" },
            new Setting { Id = 6, Key = "alert_email",          Value = "" },
            new Setting { Id = 7, Key = "alert_copie_client",   Value = "false" },
            new Setting { Id = 8, Key = "alert_resume_hebdo",   Value = "false" }
        );
    }
}
