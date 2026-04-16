using Maintenance.API.Utils;
using Xunit;

namespace Maintenance.Tests;

public class ExcelParserTests
{
    private readonly string _fixturePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "Fixtures", "contrats-test.xlsx");

    [Fact]
    public void Parse_Returns_Contracts_From_All_Sheets()
    {
        var bytes = File.ReadAllBytes(_fixturePath);
        var contracts = ExcelParser.Parse(bytes);
        Assert.True(contracts.Count > 0);
    }

    [Fact]
    public void Parse_Maps_IdPropriete_Correctly()
    {
        var bytes = File.ReadAllBytes(_fixturePath);
        var contracts = ExcelParser.Parse(bytes);
        Assert.All(contracts, c => Assert.False(string.IsNullOrWhiteSpace(c.IdPropriete)));
    }

    [Fact]
    public void Parse_Concatenates_Address_Fields()
    {
        var bytes = File.ReadAllBytes(_fixturePath);
        var contracts = ExcelParser.Parse(bytes);
        Assert.All(contracts, c => Assert.False(string.IsNullOrWhiteSpace(c.Adresse)));
    }

    [Fact]
    public void Parse_Ignores_Header_Rows_And_Empty_Rows()
    {
        var bytes = File.ReadAllBytes(_fixturePath);
        var contracts = ExcelParser.Parse(bytes);
        Assert.DoesNotContain(contracts, c => c.IdPropriete == "Propriété");
    }

    [Fact]
    public void Parse_Parses_Dates_Correctly()
    {
        var bytes = File.ReadAllBytes(_fixturePath);
        var contracts = ExcelParser.Parse(bytes);
        var withDateFin = contracts.Where(c => c.DateFin.HasValue).ToList();
        Assert.True(withDateFin.Count > 0);
        Assert.All(withDateFin, c => Assert.True(c.DateFin!.Value.Year >= 2024));
    }
}
