namespace Maintenance.API.DTOs;

public record ContractDto(
    int Id,
    string IdPropriete,
    string TypeBien,
    string Adresse,
    string Prestation,
    string Prestataire,
    decimal MontantHtAnnuel,
    DateTime? DateDebut,
    DateTime? DateFin,
    int DelaiPreavisMois,
    DateTime? DateDenonciation,
    string StatutDenonciation,   // "ok" | "bientot" | "depasse"
    string Source,
    string StatutValidation
);
