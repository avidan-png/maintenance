using Maintenance.API.Data;
using Maintenance.API.Models;
using Maintenance.API.Services;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.Tests;

public class AlertsServiceTests
{
    private static AppDbContext BuildDb(string name)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        var db = new AppDbContext(opts);
        db.Settings.AddRange(
            new Setting { Id = 1, Key = "preavis_default_mois", Value = "3" },
            new Setting { Id = 2, Key = "alert_6mois",          Value = "true" },
            new Setting { Id = 3, Key = "alert_3mois",          Value = "true" },
            new Setting { Id = 4, Key = "alert_1mois",          Value = "true" },
            new Setting { Id = 5, Key = "alert_depasse",        Value = "true" },
            new Setting { Id = 6, Key = "alert_email",          Value = "test@monga.io" },
            new Setting { Id = 7, Key = "alert_copie_client",   Value = "false" },
            new Setting { Id = 8, Key = "alert_resume_hebdo",   Value = "false" }
        );
        db.SaveChanges();
        return db;
    }

    [Fact]
    public async Task GetAlertSummary_ReturnsDepasseContracts()
    {
        var db = BuildDb("alerts_depasse");
        var today = DateTime.UtcNow.Date;
        db.Contracts.Add(new Contract
        {
            Prestation = "Toiture",
            Prestataire = "ABC",
            Adresse = "Paris",
            DateFin = today.AddMonths(1),
            DateDenonciation = today.AddDays(-10),
            StatutDenonciation = StatutDenonciation.Depasse
        });
        db.SaveChanges();

        var svc = new AlertsService(db);
        var summary = await svc.GetAlertSummaryAsync();

        Assert.Single(summary.Depasse);
        Assert.Empty(summary.Bientot);
    }

    [Fact]
    public async Task GetAlertSummary_ReturnsBientotContracts()
    {
        var db = BuildDb("alerts_bientot");
        var today = DateTime.UtcNow.Date;
        db.Contracts.Add(new Contract
        {
            Prestation = "Ascenseur",
            Prestataire = "XYZ",
            Adresse = "Lyon",
            DateFin = today.AddMonths(7),
            DateDenonciation = today.AddMonths(3),
            StatutDenonciation = StatutDenonciation.Bientot
        });
        db.SaveChanges();

        var svc = new AlertsService(db);
        var summary = await svc.GetAlertSummaryAsync();

        Assert.Empty(summary.Depasse);
        Assert.Single(summary.Bientot);
    }

    [Fact]
    public async Task GetAlertSettings_ReturnsCorrectValues()
    {
        var db = BuildDb("alerts_settings");
        var svc = new AlertsService(db);

        var settings = await svc.GetAlertSettingsAsync();

        Assert.True(settings.Alert6Mois);
        Assert.Equal("test@monga.io", settings.Email);
        Assert.False(settings.CopieCLient);
    }

    [Fact]
    public async Task UpdateAlertSettings_PersistsChanges()
    {
        var db = BuildDb("alerts_update");
        var svc = new AlertsService(db);

        await svc.UpdateAlertSettingsAsync(new Maintenance.API.DTOs.AlertSettingsDto(
            Alert6Mois: false,
            Alert3Mois: true,
            Alert1Mois: true,
            AlertDepasse: true,
            Email: "new@monga.io",
            CopieCLient: true,
            ResumeHebdo: true
        ));

        var updated = await svc.GetAlertSettingsAsync();
        Assert.False(updated.Alert6Mois);
        Assert.Equal("new@monga.io", updated.Email);
        Assert.True(updated.CopieCLient);
    }
}
