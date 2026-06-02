using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Runs;

namespace STS2_Starborn.Patches;

public sealed class KiboLoadPatch : IPatchMethod
{
    public static string PatchId => "sts2_starborn_kibo_load";
    public static string Description => "Rebuild Kibo storage from persisted data on run load";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return
        [
            new(typeof(RunManager), "InitializeSavedRun"),
        ];
    }

    public static void Postfix(RunManager __instance)
    {
        var state = __instance.DebugOnlyGetState();
        if (state == null) return;

        foreach (var player in state.Players)
        {
            var data = KiboRunData.Get(player);
            if (data == null) return;

            foreach (var typeIdStr in data.OwnedKiboTypeIds)
            {
                if (!KiboTypeId.TryParse(typeIdStr, out var typeId))
                    continue;

                TaskHelper.RunSafely(
                    KiboPileManager.CreateMasterCards(player, typeId));
            }
        }
    }
}
