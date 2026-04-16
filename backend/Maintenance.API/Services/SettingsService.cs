using Maintenance.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Maintenance.API.Services;

public class SettingsService(AppDbContext db)
{
    public async Task<int> GetPreavisDefaultAsync()
    {
        var setting = await db.Settings.FirstOrDefaultAsync(s => s.Key == "preavis_default_mois");
        return setting != null && int.TryParse(setting.Value, out var val) ? val : 3;
    }

    public async Task SetPreavisDefaultAsync(int mois)
    {
        var setting = await db.Settings.FirstOrDefaultAsync(s => s.Key == "preavis_default_mois");
        if (setting == null)
        {
            db.Settings.Add(new Models.Setting { Key = "preavis_default_mois", Value = mois.ToString() });
        }
        else
        {
            setting.Value = mois.ToString();
        }
        await db.SaveChangesAsync();
    }
}
