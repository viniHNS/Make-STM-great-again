using System.Security.Cryptography;
using System.Text;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services.Modding.Custom;

namespace MakeSTMGreatAgain;

/// <summary>
/// Creates the STM-9 Full Auto (a clone of the STM-9 with full auto, the M4A1 fire mode switch animation and the MPX
/// firing sound), its default preset, and sells it at Mechanic: barter at LL1, roubles at LL2.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.Preload + 1)]
public class Stm9FullAutoLoader(
    ISptLogger<Stm9FullAutoLoader> logger,
    CustomItemService customItemService,
    TemplateTable templateTable,
    GlobalTable globalTable,
    LocaleTable localeTable,
    TradersTable tradersTable,
    ModConfigLoader configLoader) : IOnLoad
{
    // Fixed ids: profiles keep references to them, so they must never change.
    private static readonly MongoId Stm9FullAutoId = new("b56e4c0ba0abc74ddec8e121");
    private static readonly MongoId PresetId = new("0c9ba2282474c366ef02e0fd");
    private static readonly MongoId MechanicBarterOfferId = new("6e7807bd58914707e463926a");
    private static readonly MongoId MechanicCashOfferId = new("6bf80cced23b42bf7ab87561");

    private static readonly MongoId Stm9Id = new("60339954d62c9b14ed777c06");
    private static readonly MongoId Stm9DefaultPresetId = new("60479c3f420fac5ebc199f86");
    private const string SmgParentId = "5447b5e04bdc2d62278b4567";
    private const string HandbookSmgsId = "5b5f796a86f774093f2ed3c0";

    // Copy of the vanilla container with the M4A1 fire mode switch and the MPX firing sound (bundles/ + bundles.json)
    private const string FullAutoContainerBundle = "weapon_stmarms_stm_9_9x19_auto_container.bundle";

    private const double FullAutoRpm = 840;
    private const double NewErgonomics = 53;

    private static readonly MongoId WeaponPartsId = new("5d1c819a86f774771b0acd6c");
    private static readonly MongoId GunpowderKiteId = new("590c5a7286f7747884343aea");
    private const double CashPrice = 52000;

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var result = customItemService.CreateItemFromClone(new NewItemFromCloneDetails
        {
            NewId = Stm9FullAutoId,
            ItemTplToClone = Stm9Id,
            ParentId = SmgParentId,
            NewItemName = "weapon_stmarms_stm_9_9x19_fullauto",
            HandbookParentId = HandbookSmgsId,
            HandbookPriceRoubles = 15000,
            FleaPriceRoubles = 49750,
            Locales = new()
            {
                ["en"] = new LocaleDetails
                {
                    Name = "Soyuz-TM STM-9 Gen.2 9x19 carbine (Full Auto)",
                    ShortName = "STM-9 FA",
                    Description = "A full auto conversion of the STM-9 Gen.2 PCC carbine by Soyuz-TM Arms, put together by Mechanic. " +
                                  "Keeps the accuracy and soft recoil of the semi-auto version and adds a full auto mode at 840 RPM. Compatible with Glock 9x19 magazines.",
                },
                ["po"] = new LocaleDetails
                {
                    Name = "Carabina STM-9 Gen.2 9x19 (Automática)",
                    ShortName = "STM-9 FA",
                    Description = "Uma conversão para tiro automático da carabina STM-9 Gen.2, feita pelo Mechanic. " +
                                  "Mantém a precisão e o recuo suave da versão semiautomática e ganha o modo automático a 840 RPM. Compatível com carregadores Glock 9x19.",
                },
            },
        });

        if (!result.Success)
        {
            logger.Error($"[MakeSTMGreatAgain] Failed to create the STM-9 Full Auto: {string.Join("; ", result.Errors ?? [])}");
            return Task.CompletedTask;
        }

        // Applied after the clone instead of through OverrideProperties, which also copies non-nullable defaults.
        var props = templateTable.Items[Stm9FullAutoId].Properties!;
        props.WeapFireType = ["single", "fullauto"];
        props.BFirerate = FullAutoRpm;
        props.Ergonomics = NewErgonomics;
        props.Prefab = new Prefab { Path = FullAutoContainerBundle, Rcid = "" };

        ShareStm9Mastering();

        var build = CreateDefaultPreset();
        if (build == null)
        {
            return Task.CompletedTask;
        }

        AddToMechanic(build);

        if (configLoader.Config.Debug)
        {
            logger.Info($"[MakeSTMGreatAgain] STM-9 Full Auto added ({Stm9FullAutoId}): {FullAutoRpm} RPM, preset {PresetId}, " +
                        $"Mechanic offers {MechanicBarterOfferId} (LL1 barter) and {MechanicCashOfferId} (LL2, {CashPrice} RUB).");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// The clone levels up the same mastery as the STM-9.
    /// </summary>
    private void ShareStm9Mastering()
    {
        var mastering = globalTable.Configuration.Mastering.FirstOrDefault(m => m.Templates.Contains(Stm9Id));
        if (mastering == null)
        {
            if (configLoader.Config.Debug)
            {
                logger.Warning("[MakeSTMGreatAgain] STM-9 mastering group not found, the Full Auto will have its own level.");
            }

            return;
        }

        if (!mastering.Templates.Contains(Stm9FullAutoId))
        {
            mastering.Templates = mastering.Templates.Append(Stm9FullAutoId).ToList();
        }
    }

    /// <summary>
    /// Default preset (_encyclopedia) = the STM-9 default build on the Full Auto receiver. 
    /// </summary>
    private List<Item>? CreateDefaultPreset()
    {
        if (!globalTable.ItemPresets.TryGetValue(Stm9DefaultPresetId, out var stm9Preset))
        {
            logger.Error($"[MakeSTMGreatAgain] STM-9 default preset ({Stm9DefaultPresetId}) not found. The Full Auto will not be sold.");
            return null;
        }

        var items = CloneBuild(stm9Preset.Items, PresetId.ToString(), PresetId);
        globalTable.ItemPresets[PresetId] = new Preset
        {
            Id = PresetId,
            Type = "Preset",
            ChangeWeaponName = false,
            Name = "STM-9 Full Auto Default",
            Parent = items[0].Id,
            Items = items,
            Encyclopedia = Stm9FullAutoId,
        };

        // Without the "<presetId>" key the client shows the preset id as the weapon name.
        foreach (var locale in localeTable.Global.Values)
        {
            locale.AddTransformer(data =>
            {
                if (data != null)
                {
                    data[PresetId.ToString()] = "";
                }

                return data;
            });
        }

        return stm9Preset.Items;
    }

    private void AddToMechanic(List<Item> stm9Build)
    {
        if (!tradersTable.TryGetValue(Traders.MECHANIC, out var mechanic) || mechanic?.Assort == null)
        {
            logger.Error("[MakeSTMGreatAgain] Mechanic not found, the STM-9 Full Auto will not be sold.");
            return;
        }

        AddOffer(mechanic.Assort, stm9Build, MechanicBarterOfferId, 1, buyRestriction: 2,
            [new BarterScheme { Count = 2, Template = WeaponPartsId }, new BarterScheme { Count = 1, Template = GunpowderKiteId }]);
        AddOffer(mechanic.Assort, stm9Build, MechanicCashOfferId, 2, buyRestriction: 3,
            [new BarterScheme { Count = CashPrice, Template = Money.ROUBLES }]);
    }

    private static void AddOffer(TraderAssort assort, List<Item> stm9Build, MongoId offerId, int loyaltyLevel, int buyRestriction, List<BarterScheme> cost)
    {
        if (assort.Items.Any(item => item.Id == offerId))
        {
            return;
        }

        var items = CloneBuild(stm9Build, offerId.ToString(), offerId);
        items[0].ParentId = "hideout";
        items[0].SlotId = "hideout";
        items[0].Upd = new Upd
        {
            UnlimitedCount = true,
            StackObjectsCount = 9999999,
            BuyRestrictionMax = buyRestriction,
            BuyRestrictionCurrent = 0,
            FireMode = new UpdFireMode { FireMode = "single" },
        };

        assort.Items.AddRange(items);
        assort.BarterScheme[offerId] = [cost];
        assort.LoyalLevelItems[offerId] = loyaltyLevel;
    }

    /// <summary>
    /// Copies a STM-9 build with the Full Auto as root. The root gets <paramref name="rootId"/> and the parts get ids
    /// derived from <paramref name="seed"/>, so they are the same on every start.
    /// </summary>
    private static List<Item> CloneBuild(List<Item> source, string seed, MongoId rootId)
    {
        var rootSourceId = source[0].Id.ToString();
        var ids = source.ToDictionary(item => item.Id.ToString(),
            item => item.Id.ToString() == rootSourceId ? rootId : StableId($"{seed}:{item.Id}"));

        return source.Select(item => new Item
        {
            Id = ids[item.Id.ToString()],
            Template = item.Id.ToString() == rootSourceId ? Stm9FullAutoId : item.Template,
            ParentId = item.ParentId != null && ids.TryGetValue(item.ParentId, out var parent) ? parent.ToString() : item.ParentId,
            SlotId = item.SlotId,
            Upd = item.Upd,
        }).ToList();
    }

    private static MongoId StableId(string key)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes($"makestmgreatagain:{key}"));
        return new MongoId(Convert.ToHexString(hash, 0, 12).ToLowerInvariant());
    }
}
