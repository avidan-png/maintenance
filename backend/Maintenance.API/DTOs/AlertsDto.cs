// backend/Maintenance.API/DTOs/AlertsDto.cs
namespace Maintenance.API.DTOs;

public record AlertContractDto(
    int Id,
    string Prestation,
    string Prestataire,
    string Adresse,
    DateTime? DateDenonciation,
    DateTime? DateFin
);

public record AlertSummaryDto(
    List<AlertContractDto> Depasse,
    List<AlertContractDto> Bientot
);

public record AlertSettingsDto(
    bool Alert6Mois,
    bool Alert3Mois,
    bool Alert1Mois,
    bool AlertDepasse,
    string Email,
    bool CopieCLient,
    bool ResumeHebdo
);
