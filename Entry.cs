using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;
using STS2_Starborn.Cards.Common;
using STS2_Starborn.Kibo;
using STS2_Starborn.Kibo.Patches;
using STS2_Starborn.Relics;

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

        RitsuLibFramework.RegisterArchaicToothTranscendenceMapping<TestCard, TestAncientCard>();
        RitsuLibFramework.RegisterTouchOfOrobasRefinementMapping<TestRelic, TestRelic>();

        KiboTypeRegistry.Initialize();
        KiboRunData.Initialize();
        KiboPileManager.RegisterPile();

        var patcher = RitsuLibFramework.CreatePatcher(Const.ModId, "main");
        patcher.RegisterPatch<KiboAutoSummonPatch>();
        RitsuLibFramework.ApplyRequiredPatcher(patcher, DisableMod);
    }

    private static void DisableMod()
    {
    }
}
