using Maintenance.API.Models;
using Maintenance.API.Utils;
using Xunit;

namespace Maintenance.Tests;

public class DenonciationCalculatorTests
{
    private static readonly DateTime Today = new DateTime(2026, 4, 15);

    [Fact]
    public void Compute_DateDenonciation_Is_DateFin_Minus_DelaiMois()
    {
        var result = DenonciationCalculator.Compute(new DateTime(2026, 12, 31), 3, Today);
        Assert.Equal(new DateTime(2026, 9, 30), result.DateDenonciation);
    }

    [Fact]
    public void Statut_Is_Depasse_When_DateDenonciation_Past()
    {
        var result = DenonciationCalculator.Compute(new DateTime(2025, 12, 31), 3, Today);
        Assert.Equal(StatutDenonciation.Depasse, result.Statut);
    }

    [Fact]
    public void Statut_Is_Bientot_When_DateDenonciation_Within_6_Months()
    {
        var result = DenonciationCalculator.Compute(new DateTime(2026, 9, 30), 3, Today);
        Assert.Equal(StatutDenonciation.Bientot, result.Statut);
    }

    [Fact]
    public void Statut_Is_Ok_When_DateDenonciation_After_6_Months()
    {
        var result = DenonciationCalculator.Compute(new DateTime(2027, 6, 30), 3, Today);
        Assert.Equal(StatutDenonciation.Ok, result.Statut);
    }

    [Fact]
    public void Handles_EndOfMonth_Correctly_Jan_Minus_3_Is_Oct()
    {
        var result = DenonciationCalculator.Compute(new DateTime(2027, 1, 31), 3, Today);
        Assert.Equal(new DateTime(2026, 10, 31), result.DateDenonciation);
    }
}
