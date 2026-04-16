using ClosedXML.Excel;

namespace Maintenance.API.Utils;

public record RawContractRow(
    string IdPropriete,
    string TypeBien,
    string Adresse,
    string Prestation,
    string Prestataire,
    decimal MontantHtAnnuel,
    DateTime? DateDebut,
    DateTime? DateFin
);

public static class ExcelParser
{
    // Colonnes attendues (avec les espaces tels quels dans les fichiers clients)
    private static readonly string[] ColPropriete   = { " Propriété", "Propriété", "Propriete" };
    private static readonly string[] ColTypeBien    = { "Nature des locaux" };
    private static readonly string[] ColAdresse1    = { "Adresse 1" };
    private static readonly string[] ColAdresse2    = { "Adresse 2" };
    private static readonly string[] ColCp          = { "CP" };
    private static readonly string[] ColVille       = { "Ville" };
    private static readonly string[] ColPrestation  = { " Libellé contrat", "Libellé contrat", "Libelle contrat" };
    private static readonly string[] ColPrestataire = { " Libellé fournisseur", "Libellé fournisseur" };
    private static readonly string[] ColMontant     = { " Montant HT / an", "Montant HT / an" };
    private static readonly string[] ColDateDebut   = { " Date début renouv.", "Date début renouv." };
    private static readonly string[] ColDateFin     = { " Date fin renouv.", "Date fin renouv." };

    public static List<RawContractRow> Parse(byte[] fileBytes)
    {
        var results = new List<RawContractRow>();

        using var stream = new MemoryStream(fileBytes);
        using var workbook = new XLWorkbook(stream);

        foreach (var worksheet in workbook.Worksheets)
        {
            var headerRow = FindHeaderRow(worksheet);
            if (headerRow == null) continue;

            var colIndex = BuildColumnIndex(headerRow);

            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
            for (int row = headerRow.RowNumber() + 1; row <= lastRow; row++)
            {
                var wsRow = worksheet.Row(row);
                var idPropriete = GetString(wsRow, colIndex, ColPropriete);
                if (string.IsNullOrWhiteSpace(idPropriete) || idPropriete.Contains("Propriété"))
                    continue;

                var adresse = BuildAdresse(
                    GetString(wsRow, colIndex, ColAdresse1),
                    GetString(wsRow, colIndex, ColAdresse2),
                    GetString(wsRow, colIndex, ColCp),
                    GetString(wsRow, colIndex, ColVille)
                );

                results.Add(new RawContractRow(
                    IdPropriete:     idPropriete,
                    TypeBien:        GetString(wsRow, colIndex, ColTypeBien),
                    Adresse:         adresse,
                    Prestation:      GetString(wsRow, colIndex, ColPrestation),
                    Prestataire:     GetString(wsRow, colIndex, ColPrestataire),
                    MontantHtAnnuel: GetDecimal(wsRow, colIndex, ColMontant),
                    DateDebut:       GetDate(wsRow, colIndex, ColDateDebut),
                    DateFin:         GetDate(wsRow, colIndex, ColDateFin)
                ));
            }
        }

        return results;
    }

    private static IXLRow? FindHeaderRow(IXLWorksheet ws)
    {
        foreach (var row in ws.RowsUsed())
        {
            if (row.Cells().Any(c => c.GetString().Contains("Propriét")))
                return row;
        }
        return null;
    }

    private static Dictionary<string, int> BuildColumnIndex(IXLRow headerRow)
    {
        var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in headerRow.CellsUsed())
            index[cell.GetString().Trim()] = cell.Address.ColumnNumber;
        return index;
    }

    private static string GetString(IXLRow row, Dictionary<string, int> index, string[] candidates)
    {
        foreach (var key in candidates)
            if (index.TryGetValue(key.Trim(), out var col))
                return row.Cell(col).GetString().Trim();
        return string.Empty;
    }

    private static decimal GetDecimal(IXLRow row, Dictionary<string, int> index, string[] candidates)
    {
        var raw = GetString(row, index, candidates);
        return decimal.TryParse(raw, out var val) ? val : 0m;
    }

    private static DateTime? GetDate(IXLRow row, Dictionary<string, int> index, string[] candidates)
    {
        foreach (var key in candidates)
        {
            if (!index.TryGetValue(key.Trim(), out var col)) continue;
            var cell = row.Cell(col);
            if (cell.DataType == XLDataType.DateTime) return cell.GetDateTime();
            if (DateTime.TryParse(cell.GetString(), out var dt)) return dt;
        }
        return null;
    }

    private static string BuildAdresse(string a1, string a2, string cp, string ville)
    {
        var parts = new List<string> { a1 };
        if (!string.IsNullOrWhiteSpace(a2) && a2 != "0") parts.Add(a2);
        if (!string.IsNullOrWhiteSpace(cp)) parts.Add(cp);
        if (!string.IsNullOrWhiteSpace(ville)) parts.Add(ville);
        return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }
}
