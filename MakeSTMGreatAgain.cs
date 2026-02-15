using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace MakeSTMGreatAgain;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.vinihns.makestmgreatagain";
    public override string Name { get; init; } = "Make STM Great Again";
    public override string Author { get; init; } = "ViniHNS";
    public override SemanticVersioning.Version Version { get; init; } = new("1.0.1"); 
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override string? License { get; init; } = "MIT";
    
    public override List<string>? Contributors { get; init; }
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; } = "https://github.com/viniHNS/Make-STM-great-again";
    public override bool? IsBundleMod { get; init; } = false;
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class Mod(
    ISptLogger<Mod> logger,
    DatabaseService databaseService)
    : IOnLoad
{
    // ID (TPL) da VPO-101
    private const string STM9_ID = "60339954d62c9b14ed777c06";
    
    public Task OnLoad()
    {
        logger.Info("[ViniHNS] Making the STM9 great again!");

        var items = databaseService.GetItems();
        
        if (!items.TryGetValue(STM9_ID, out var stm9))
        {
            logger.Error($"Could not find STM9 ({STM9_ID}). Aborting.");
            return Task.CompletedTask;
        }
        
        
        // Add fullauto fire mode, increase ergonomics and increase bfirerate
        if (stm9.Properties != null)
        {
            stm9.Properties.Ergonomics = 53;
            if (stm9.Properties.WeapFireType != null && !stm9.Properties.WeapFireType.Contains("fullauto"))
            {
                stm9.Properties.WeapFireType.Add("fullauto");
            }
            stm9.Properties.BFirerate = 800; // 800 RPM
        }

        return Task.CompletedTask;
    }
}