// backend/Maintenance.API/Services/AlertsService.cs
using Maintenance.API.Data;
using Maintenance.API.DTOs;
using Maintenance.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.API.Services;

public class AlertsService(AppDbContext db)
{
    public async Task<AlertSummaryDto> GetAlertSummaryAsync()
    {
        var contracts = await db.Contracts
            .Where(c => c.StatutDenonciation != StatutDenonciation.Ok)
            .ToListAsync();

        var depasse = contracts
            .Where(c => c.StatutDenonciation == StatutDenonciation.Depasse)
            .Select(ToAlertDto)
            .ToList();

        var bientot = contracts
            .Where(c => c.StatutDenonciation == StatutDenonciation.Bientot)
            .Select(ToAlertDto)
            .ToList();

        return new AlertSummaryDto(depasse, bientot);
    }

    public async Task<AlertSettingsDto> GetAlertSettingsAsync()
    {
        var keys = new[] {
            "alert_6mois", "alert_3mois", "alert_1mois", "alert_depasse",
            "alert_email", "alert_copie_client", "alert_resume_hebdo"
        };
        var settings = await db.Settings
            .Where(s => keys.Contains(s.Key))
            .ToDictionaryAsync(s => s.Key, s => s.Value);

        return new AlertSettingsDto(
            Alert6Mois:   GetBool(settings, "alert_6mois", true),
            Alert3Mois:   GetBool(settings, "alert_3mois", true),
            Alert1Mois:   GetBool(settings, "alert_1mois", true),
            AlertDepasse: GetBool(settings, "alert_depasse", true),
            Email:        settings.GetValueOrDefault("alert_email", ""),
            CopieCLient:  GetBool(settings, "alert_copie_client", false),
            ResumeHebdo:  GetBool(settings, "alert_resume_hebdo", false)
        );
    }

    public async Task UpdateAlertSettingsAsync(AlertSettingsDto dto)
    {
        var updates = new Dictionary<string, string>
        {
            ["alert_6mois"]        = dto.Alert6Mois.ToString().ToLower(),
            ["alert_3mois"]        = dto.Alert3Mois.ToString().ToLower(),
            ["alert_1mois"]        = dto.Alert1Mois.ToString().ToLower(),
            ["alert_depasse"]      = dto.AlertDepasse.ToString().ToLower(),
            ["alert_email"]        = dto.Email,
            ["alert_copie_client"] = dto.CopieCLient.ToString().ToLower(),
            ["alert_resume_hebdo"] = dto.ResumeHebdo.ToString().ToLower(),
        };

        foreach (var (key, value) in updates)
        {
            var setting = await db.Settings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting != null) setting.Value = value;
        }

        await db.SaveChangesAsync();
    }

    private static bool GetBool(Dictionary<string, string> d, string key, bool def)
        => d.TryGetValue(key, out var v) ? v == "true" : def;

    private static AlertContractDto ToAlertDto(Contract c) => new(
        c.Id, c.Prestation, c.Prestataire, c.Adresse, c.DateDenonciation, c.DateFin
    );
}
