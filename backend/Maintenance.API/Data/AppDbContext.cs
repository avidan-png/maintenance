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

        // Seed: délai de préavis par défaut = 3 mois
        modelBuilder.Entity<Setting>().HasData(
            new Setting { Id = 1, Key = "preavis_default_mois", Value = "3" }
        );
    }
}
