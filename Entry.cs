using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;
using STS2_Starborn.Cards;
using STS2_Starborn.Cards.Common;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Character;
using STS2_Starborn.Patches;
using STS2_Starborn.Relics;
using STS2_Starborn.Runs;

namespace STS2_Starborn;

[ModInitializer(nameof(Init))]
public class Entry
{
    public static readonly Logger Logger = RitsuLibFramework.CreateLogger(Const.ModId);

    public static void Init()
    {
        var assembly = Assembly.GetExecutingAssembly();
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        ModTypeDiscoveryHub.RegisterModAssembly(Const.ModId, assembly);

        RitsuLibFramework.RegisterTouchOfOrobasRefinementMapping<StarBoundCardRelic, WanderingStarlinesRelic>();

        KiboTypeRegistry.Initialize();
        KiboRunData.Initialize();
        KiboPileManager.RegisterPiles();

        var starbornEntry = ModelDb.GetEntry(typeof(Starborn));
        var contentRegistry = RitsuLibFramework.GetContentRegistry(Const.ModId);
        contentRegistry.RegisterCardLibraryCompendiumSharedPoolFilter<KiboCardPool>(
            "kibo",
            Const.Paths.KiboPileIcon,
            [
                new CardLibraryCompendiumPlacementRule
                {
                    ModCharacterModelIdEntry = starbornEntry,
                    Relation = CardLibraryCompendiumFilterInsertRelation.After,
                },
            ]);
        contentRegistry.RegisterCardHandOutline<StarbornCard>(
            card => card.ResolveTuningGlowColorOrNull(),
            refreshEveryFrame: true);

        var patcher = RitsuLibFramework.CreatePatcher(Const.ModId, "main");
        patcher.RegisterPatch<KiboAutoSummonPatch>();
        patcher.RegisterPatch<KiboCardPlayHookFilterPatch>();
        patcher.RegisterPatch<KiboDamageModifierPatch>();
        patcher.RegisterPatch<KiboDamageReflectorPatch>();
        patcher.RegisterPatch<WidgetCombatUiReadyPatch>();
        patcher.RegisterPatch<WidgetCombatUiActivatePatch>();
        patcher.RegisterPatch<KiboLoadPatch>();
        patcher.RegisterPatch<CompactCardGridHoverTipPatch>();
        patcher.RegisterPatch<KiboAncientVisualPatch>();
#if DEBUG
        patcher.RegisterPatch<StarbornDebugPanelPatch>();
#endif
        RitsuLibFramework.ApplyRequiredPatcher(patcher, DisableMod);
    }

    private static void DisableMod()
    {
    }
}
