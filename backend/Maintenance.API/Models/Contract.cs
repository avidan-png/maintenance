namespace Maintenance.API.Models;

public enum StatutDenonciation { Ok, Bientot, Depasse }
public enum SourceContrat { Monga, Externe }
public enum StatutValidation { Valide, EnAttente }

public class Contract
{
    public int Id { get; set; }
    public string IdPropriete { get; set; } = string.Empty;
    public string TypeBien { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public string Prestation { get; set; } = string.Empty;
    public string Prestataire { get; set; } = string.Empty;
    public string? AdressePrestataire { get; set; }
    public decimal MontantHtAnnuel { get; set; }
    public DateTime? DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
    public int DelaiPreavisMois { get; set; } = 3;
    public DateTime? DateDenonciation { get; set; }
    public StatutDenonciation StatutDenonciation { get; set; } = StatutDenonciation.Ok;
    public SourceContrat Source { get; set; } = SourceContrat.Externe;
    public StatutValidation StatutValidation { get; set; } = StatutValidation.Valide;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
