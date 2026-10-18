using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace MakeSTMGreatAgain;

/// <summary>
/// Small buffs for the vanilla STM-9.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class Stm9Changes(
    ISptLogger<Stm9Changes> logger,
    TemplateTable templateTable,
    ModConfigLoader configLoader) : IOnLoad
{
    // STM-9 Gen.2 9x19 carbine
    private const string Stm9Tpl = "60339954d62c9b14ed777c06";

    private const double NewErgonomics = 53;

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        if (!templateTable.Items.TryGetValue(Stm9Tpl, out var stm9) || stm9.Properties == null)
        {
            logger.Error($"[MakeSTMGreatAgain] Could not find STM-9 ({Stm9Tpl}). Aborting.");
            return Task.CompletedTask;
        }

        stm9.Properties.Ergonomics = NewErgonomics;

        if (configLoader.Config.Debug)
        {
            logger.Info($"[MakeSTMGreatAgain] STM-9 ergonomics set to {NewErgonomics}.");
        }

        return Task.CompletedTask;
    }
}
