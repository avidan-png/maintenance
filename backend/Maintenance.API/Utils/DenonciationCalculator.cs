using Maintenance.API.Models;

namespace Maintenance.API.Utils;

public record DenonciationResult(DateTime DateDenonciation, StatutDenonciation Statut);

public static class DenonciationCalculator
{
    public static DenonciationResult Compute(DateTime dateFin, int delaiMois, DateTime? today = null)
    {
        var reference = today ?? DateTime.UtcNow.Date;
        var dateDenonciation = dateFin.AddMonths(-delaiMois);

        StatutDenonciation statut;
        if (dateDenonciation < reference)
            statut = StatutDenonciation.Depasse;
        else if (dateDenonciation <= reference.AddMonths(6))
            statut = StatutDenonciation.Bientot;
        else
            statut = StatutDenonciation.Ok;

        return new DenonciationResult(dateDenonciation, statut);
    }
}
