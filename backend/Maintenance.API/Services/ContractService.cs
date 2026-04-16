using Maintenance.API.Data;
using Maintenance.API.DTOs;
using Maintenance.API.Models;
using Maintenance.API.Utils;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Maintenance.API.Services;

public class ContractService(AppDbContext db, SettingsService settings)
{
    public async Task<List<ContractDto>> GetAllAsync()
    {
        var contracts = await db.Contracts.ToListAsync();
        return contracts.Select(ToDto).ToList();
    }

    public async Task<ImportResultDto> ImportFromExcelAsync(byte[] fileBytes, int? delaiOverride = null)
    {
        var delai = delaiOverride ?? await settings.GetPreavisDefaultAsync();
        var rows = ExcelParser.Parse(fileBytes);
        var imported = 0;

        foreach (var row in rows)
        {
            DateTime? dateDenonciation = null;
            var statut = StatutDenonciation.Ok;

            if (row.DateFin.HasValue)
            {
                var computed = DenonciationCalculator.Compute(row.DateFin.Value, delai);
                dateDenonciation = computed.DateDenonciation;
                statut = computed.Statut;
            }

            // Parse montant avec support locale FR (virgule comme séparateur décimal)
            var montant = row.MontantHtAnnuel != 0m
                ? row.MontantHtAnnuel
                : 0m;

            db.Contracts.Add(new Contract
            {
                IdPropriete        = row.IdPropriete,
                TypeBien           = row.TypeBien,
                Adresse            = row.Adresse,
                Prestation         = row.Prestation,
                Prestataire        = row.Prestataire,
                MontantHtAnnuel    = montant,
                DateDebut          = row.DateDebut,
                DateFin            = row.DateFin,
                DelaiPreavisMois   = delai,
                DateDenonciation   = dateDenonciation,
                StatutDenonciation = statut,
                Source             = SourceContrat.Externe,
                StatutValidation   = StatutValidation.Valide,
            });
            imported++;
        }

        await db.SaveChangesAsync();
        return new ImportResultDto(imported);
    }

    private static ContractDto ToDto(Contract c) => new(
        c.Id,
        c.IdPropriete,
        c.TypeBien,
        c.Adresse,
        c.Prestation,
        c.Prestataire,
        c.MontantHtAnnuel,
        c.DateDebut,
        c.DateFin,
        c.DelaiPreavisMois,
        c.DateDenonciation,
        c.StatutDenonciation.ToString().ToLower(),
        c.Source.ToString().ToLower(),
        c.StatutValidation.ToString().ToLower()
    );
}

public record ImportResultDto(int Imported);
